using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TagDamage
{
    public string tag;
    public float damage = 100f;
}

public class HomingBullet : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float dropSpeed;
    private float lifeTime;
    private GameObject explosionPrefab;
    private bool exploded = false;

    [Header("Explosion Settings")]
    public float maxExplosionRadius = 5f;        // 最大爆炸半径
    public float expansionTime = 0.5f;           // 爆炸从小到大持续时间

    [Header("Default Damage Settings")]
    public float defaultExplosionDamage = 100f;  // 默认爆炸中心伤害
    public bool useDistanceFalloff = true;       // 是否使用距离衰减

    [Header("Per-Tag Damage Control")]
    [Tooltip("为不同的Tag定义单独伤害值（覆盖默认伤害）")]
    public TagDamage[] tagDamageList;

    private HashSet<GameObject> damagedTargets = new HashSet<GameObject>(); // 防止重复伤害

    // --- 初始化 ---
    public void Init(Transform target, float speed, float dropSpeed, GameObject explosionPrefab, float lifeTime)
    {
        this.target = target;
        this.speed = speed;
        this.dropSpeed = dropSpeed;
        this.explosionPrefab = explosionPrefab;
        this.lifeTime = lifeTime;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (exploded || target == null) return;

        // 追踪 + 下坠
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y -= dropSpeed * Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;
        if (other.CompareTag("Boss")) return; // 忽略Boss自身
        if (!other.isTrigger)
            Explode();
    }

    private void Explode()
    {
        exploded = true;

        // 1) 爆炸特效
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // 2) 启动爆炸伤害协程
        StartCoroutine(ExplosionDamageCoroutine());

        // 3) 延迟销毁
        Destroy(gameObject, expansionTime + 0.05f);
    }

    private IEnumerator ExplosionDamageCoroutine()
    {
        ProjectileHitHandler globalHandler = FindObjectOfType<ProjectileHitHandler>();
        if (globalHandler == null)
        {
            Debug.LogWarning("No ProjectileHitHandler found in the scene!");
            yield break;
        }

        float timer = 0f;
        while (timer < expansionTime)
        {
            float currentRadius = Mathf.Lerp(0f, maxExplosionRadius, timer / expansionTime);
            ApplyExplosionDamage(globalHandler, currentRadius);
            timer += Time.deltaTime;
            yield return null;
        }

        // 最终再检测一次最大半径
        ApplyExplosionDamage(globalHandler, maxExplosionRadius);
    }

    private void ApplyExplosionDamage(ProjectileHitHandler handler, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            GameObject go = hit.gameObject;
            if (damagedTargets.Contains(go)) continue;

            float distance = Vector3.Distance(transform.position, go.transform.position);
            float baseDamage = GetDamageForTag(go.tag);

            // ✅ 距离衰减
            if (useDistanceFalloff)
            {
                float factor = Mathf.Clamp01(1f - (distance / maxExplosionRadius));
                baseDamage *= factor;
            }

            handler.HandleHit(go, baseDamage);
            damagedTargets.Add(go);
        }
    }

    /// <summary>
    /// 根据tag获取对应伤害（未定义则使用默认伤害）
    /// </summary>
    private float GetDamageForTag(string tag)
    {
        foreach (var td in tagDamageList)
        {
            if (td != null && td.tag == tag)
                return td.damage;
        }
        return defaultExplosionDamage;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxExplosionRadius);
    }
}
