using UnityEngine;

public class BombExplosionDamage : MonoBehaviour
{
    [Header("爆炸参数")]
    public float damageRadius = 3f;
    public float delayBeforeDamage = 0f;

    [Header("引用（可选，不填自动找）")]
    public ProjectileHitHandler hitHandler;

    [Header("可破坏物体设置")]
    public string destructibleTag = "Destructible"; // ✅ 指定哪些物体可被炸毁
    public int damageHits = 1;                      // ✅ 每次爆炸造成的命中次数（比如1次）

    private Transform player;

    [Header("事件触发器")]
    [Tooltip("Boss晕厥时触发的事件（类似 OnPhaseTrigger）")]
    public UnityEngine.Events.UnityEvent onStunTrigger;

    [Tooltip("Boss从晕厥恢复时触发的事件")]
    public UnityEngine.Events.UnityEvent onWakeUpTrigger;



    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (hitHandler == null)
        {
            hitHandler = FindObjectOfType<ProjectileHitHandler>();
        }
    }

    private void Start()
    {
        if (delayBeforeDamage > 0)
            Invoke(nameof(ApplyExplosionDamage), delayBeforeDamage);
        else
            ApplyExplosionDamage();
    }

    private void ApplyExplosionDamage()
    {
        // ✅ 对玩家造成伤害
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= damageRadius && hitHandler != null)
            {
                hitHandler.HandleHit(player.gameObject);
            }
        }

        // ✅ 对带特定 Tag 的可破坏物体造成伤害
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(destructibleTag))
            {
                DestructibleObject destructible = hit.GetComponent<DestructibleObject>();
                if (destructible != null)
                {
                    for (int i = 0; i < damageHits; i++)
                        destructible.TakeHit();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
