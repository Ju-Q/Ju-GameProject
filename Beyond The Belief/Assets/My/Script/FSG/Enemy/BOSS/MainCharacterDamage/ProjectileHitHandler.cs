using UnityEngine;

public class ProjectileHitHandler : MonoBehaviour
{
    [Header("玩家命中处理")]
    public bool affectPlayer = true;   // 是否对玩家生效

    [Header("命中转化参数")]
    [Tooltip("每次调用 TakeHit() 等价的伤害值（用于把数值伤害转换为命中次数）")]
    public float damagePerHit = 50f;   // 每命中一次的等价伤害值

    /// <summary>
    /// 旧接口（兼容老逻辑）
    /// </summary>
    public void HandleHit(GameObject target)
    {
        HandleHit(target, damagePerHit);
    }

    /// <summary>
    /// 新接口：支持传入数值伤害
    /// </summary>
    public void HandleHit(GameObject target, float damage)
    {
        if (target == null) return;

        // --- 玩家逻辑 ---
        if (affectPlayer && target.CompareTag("Player"))
        {
            var ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // 如果 PlayerHealth 有 TakeDamage() 方法，优先调用
                if (HasMethod(ph, "TakeDamage"))
                    ph.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                else
                    ph.TakeHit(); // 回退到旧逻辑
            }
            return;
        }

        // --- 可破坏物体逻辑 ---
        var destructible = target.GetComponent<DestructibleObject>();
        if (destructible != null)
        {
            // 根据伤害计算命中次数（至少一次）
            int hits = Mathf.Max(1, Mathf.CeilToInt(damage / damagePerHit));
            for (int i = 0; i < hits; i++)
            {
                destructible.TakeHit();
            }
        }
    }

    // ✅ 工具：检测组件是否有某个方法（防止报错）
    private bool HasMethod(object obj, string methodName)
    {
        return obj.GetType().GetMethod(methodName) != null;
    }
}
