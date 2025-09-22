using UnityEngine;

public class BombExplosionDamage : MonoBehaviour
{
    [Header("爆炸参数")]
    public float damageRadius = 3f;
    public float delayBeforeDamage = 0f;

    [Header("引用（可选，不填自动找）")]
    public ProjectileHitHandler hitHandler;

    private Transform player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 如果 Inspector 没拖 hitHandler，就自动在场景里找
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
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= damageRadius && hitHandler != null)
        {
            hitHandler.HandleHit(player.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
