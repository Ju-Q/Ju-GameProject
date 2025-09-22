using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingBullet : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float dropSpeed;
    private float lifeTime;
    private GameObject explosionPrefab;
    private bool exploded = false;

    [Header("Explosion Damage")]
    public float maxExplosionRadius = 5f;        // 最大爆炸半径
    public float expansionTime = 0.5f;           // 爆炸从小到大持续时间

    private HashSet<GameObject> damagedTargets = new HashSet<GameObject>(); // 防止重复伤害

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

        // 追踪目标
        Vector3 direction = (target.position - transform.position).normalized;
        // 高度逐渐降低
        direction.y -= dropSpeed * Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;

        // 忽略Boss自身
        if (other.CompareTag("Boss")) return;

        // 只对非Trigger碰撞体爆炸
        if (!other.isTrigger)
        {
            Explode();
        }
    }

    private void Explode()
    {
        exploded = true;

        // 1) 播放爆炸特效
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // 2) 启动范围扩张伤害协程
        StartCoroutine(ExplosionDamageCoroutine());

        // 3) 销毁子弹（延迟一帧等待协程检测伤害）
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

            // 检测玩家是否进入当前半径
            Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius);
            foreach (Collider hit in hits)
            {
                if (damagedTargets.Contains(hit.gameObject)) continue; // 已伤害过

                if (hit.CompareTag("Player"))
                {
                    damagedTargets.Add(hit.gameObject);
                    globalHandler.HandleHit(hit.gameObject);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 最终检测最大半径
        Collider[] finalHits = Physics.OverlapSphere(transform.position, maxExplosionRadius);
        foreach (Collider hit in finalHits)
        {
            if (damagedTargets.Contains(hit.gameObject)) continue;
            if (hit.CompareTag("Player"))
            {
                globalHandler.HandleHit(hit.gameObject);
                damagedTargets.Add(hit.gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 编辑器中显示最大半径
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxExplosionRadius);
    }
}
