using UnityEngine;

public class ProjectileHitHandler : MonoBehaviour
{
    [Header("玩家命中处理")]
    public bool affectPlayer = true;   // 是否对玩家生效

    public void HandleHit(GameObject target)
    {
        // --- 玩家逻辑 ---
        if (affectPlayer && target.CompareTag("Player"))
        {
            var ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeHit(); // 玩家扣血，但不生成特效
            }
            return;
        }

        // --- 可破坏物体逻辑 ---
        var destructible = target.GetComponent<DestructibleObject>();
        if (destructible != null)
        {
            destructible.TakeHit(); // 特效交给物体自己决定
        }
    }
}
