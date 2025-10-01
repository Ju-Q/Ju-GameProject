using UnityEngine;
using System.Collections;

public abstract class EnemyAttack : MonoBehaviour
{
    /// <summary>
    /// 执行一次完整攻击的协程（子类必须实现）
    /// </summary>
    public abstract IEnumerator ExecuteAttack();

    /// <summary>
    /// 重置攻击状态（默认空实现，子类可按需重写）
    /// 在Boss回溯或重生时调用，用于停止特效、重置冷却等
    /// </summary>
    public virtual void ResetAttack()
    {
        // 默认什么都不做
        // 子类按需 override
    }
}
