// PlayerHealth.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Tooltip("被击多少次后视为死亡")]
    public int hitsToDie = 2;

    
    public int currentHits = 0;
    [HideInInspector]
    public bool isDead { get; private set; } = false;

    // 在 Inspector 可绑定：比如播放受击音效/闪红/减血UI
    public UnityEvent OnHit;
    // 在 Inspector 可绑定：比如播放死亡动画、黑屏、重置等
    public UnityEvent OnDeath;

    /// <summary>
    /// 外部调用：玩家被击一次
    /// </summary>
    public void TakeHit()
    {
        if (isDead) return;

        currentHits++;
        Debug.Log($"{gameObject.name} got hit: {currentHits}/{hitsToDie}");
        OnHit?.Invoke();

        if (currentHits >= hitsToDie)
        {
            Die();
        }
    }

    /// <summary>
    /// 达到死亡条件时调用（目前只触发事件与日志，具体效果在 OnDeath 里配置）
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} died (hit count reached).");
        OnDeath?.Invoke();
        // 注意：不要在这里直接做视觉/UI的具体实现（按你的要求），
        // 把这些效果通过 OnDeath 在 Inspector 里挂上，或在下一步我帮你实现。
    }

    /// <summary>
    /// 重置状态（比如重生时调用）
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        currentHits = 0;
    }
}
