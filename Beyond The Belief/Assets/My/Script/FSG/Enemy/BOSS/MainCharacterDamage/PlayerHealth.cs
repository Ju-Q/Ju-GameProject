using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("基础设置")]
    [Tooltip("被击多少次后视为死亡")]
    public int hitsToDie = 2;

    [Tooltip("受击后多少秒无伤害开始回血")]
    public float healDelay = 3f;

    [Tooltip("从当前血量到满血恢复需要的时间（秒）")]
    public float healDuration = 2f;

    public int currentHits = 0;

    [HideInInspector]
    public bool isDead { get; private set; } = false;

    [Header("受伤UI")]
    [Tooltip("受击时屏幕红色覆盖UI（Image）")]
    public Image damageOverlayImage;

    [Tooltip("死亡黑屏UI（Image）")]
    public Image deathBlackScreenImage;

    [Tooltip("黑屏淡入/淡出时间")]
    public float blackScreenFadeDuration = 1f;

    [Tooltip("黑屏停留时间")]
    public float blackScreenStayDuration = 2f;

    [Header("Animator设置")]
    public ThirdPersonController Controller;
    public Transform playerModel;
    public Animator playerAnimator;
    public string deathTriggerName = "BossDeath";
    public string rebirthTriggerName = "Rebirth";

    [Header("事件")]
    public UnityEvent OnHit;
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;

    private float lastHitTime;
    private bool isHealing = false;
    private float damageOverlayAlpha = 0f;

    [Header("Boss控制器")]
    public EnemyAttackController bossAttackController;
    [Tooltip("玩家重生后Boss等待多久才恢复攻击")]
    public float bossRespawnDelay = 2f;

    void Update()
    {
        // 自动回血逻辑
        if (!isDead && currentHits > 0 && !isHealing && Time.time - lastHitTime >= healDelay)
        {
            StartCoroutine(HealOverTime());
        }

        // 更新受伤 UI 透明度（平滑过渡）
        UpdateDamageOverlay();
    }

    public void TakeHit()
    {
        if (isDead) return;

        currentHits++;
        Debug.Log($"{gameObject.name} got hit: {currentHits}/{hitsToDie}");
        OnHit?.Invoke();

        lastHitTime = Time.time;
        isHealing = false;

        if (currentHits >= hitsToDie)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} died (hit count reached).");
        OnDeath?.Invoke();

        // ✅ 死亡时先暂停Boss并回溯
        if (bossAttackController != null)
        {
            Debug.Log("[DEBUG] Player died! Pausing boss & rewinding.");
            bossAttackController.CanStartAttack = false; // 强制关掉攻击
            bossAttackController.PauseBoss();
            bossAttackController.RewindToCheckpoint();
        }

        if (playerAnimator != null && !string.IsNullOrEmpty(deathTriggerName))
        {
            playerAnimator.applyRootMotion = true;
            playerAnimator.SetTrigger(deathTriggerName);
            Controller.canMove = false;
        }

        if (deathBlackScreenImage != null)
            StartCoroutine(BlackScreenFade());
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHits = 0;
        isHealing = false;

        // 受伤UI恢复透明
        if (damageOverlayImage != null)
        {
            Color c = damageOverlayImage.color;
            c.a = 0f;
            damageOverlayImage.color = c;
            damageOverlayAlpha = 0f;
        }

        Controller.canMove = true;

        OnRespawn?.Invoke();
    }

    private System.Collections.IEnumerator HealOverTime()
    {
        isHealing = true;

        float startHits = currentHits;
        float elapsed = 0f;

        while (elapsed < healDuration)
        {
            if (Time.time - lastHitTime < healDelay)
            {
                isHealing = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / healDuration;
            currentHits = Mathf.RoundToInt(Mathf.Lerp(startHits, 0, t));
            yield return null;
        }

        currentHits = 0;
        isHealing = false;
    }

    private void UpdateDamageOverlay()
    {
        if (damageOverlayImage == null) return;

        float targetAlpha = Mathf.Clamp01((float)currentHits / hitsToDie);
        damageOverlayAlpha = Mathf.Lerp(damageOverlayAlpha, targetAlpha, Time.deltaTime * 5f);

        Color c = damageOverlayImage.color;
        c.a = damageOverlayAlpha;
        damageOverlayImage.color = c;
    }

    private IEnumerator BlackScreenFade()
    {
        if (deathBlackScreenImage == null) yield break;

        // 淡入
        float elapsed = 0f;
        while (elapsed < blackScreenFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blackScreenFadeDuration;
            SetImageAlpha(deathBlackScreenImage, t);
            yield return null;
        }
        SetImageAlpha(deathBlackScreenImage, 1f);

        // 触发Animator重生动画
        if (playerAnimator != null && !string.IsNullOrEmpty(rebirthTriggerName))
        {
            playerAnimator.applyRootMotion = false;
            if (playerModel != null)
            {
                playerModel.localPosition = Vector3.zero;
                playerModel.localRotation = Quaternion.identity;
            }
            playerAnimator.SetTrigger(rebirthTriggerName);
        }

        yield return new WaitForSeconds(blackScreenStayDuration);

        ResetHealth();

        // 重置玩家位置到 checkpoint 对应位置
        if (bossAttackController != null)
        {
            Vector3 respawnPos = bossAttackController.GetRespawnPositionForCurrentPhase();
            Controller.transform.position = respawnPos;
        }

        // ✅ 延迟后只设置CanStartAttack=true，让EnemyAttackController自己Resume
        if (bossAttackController != null)
        {
            yield return new WaitForSeconds(bossRespawnDelay);
            bossAttackController.CanStartAttack = true;
        }

        // 淡出
        elapsed = 0f;
        while (elapsed < blackScreenFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / blackScreenFadeDuration);
            SetImageAlpha(deathBlackScreenImage, t);
            yield return null;
        }
        SetImageAlpha(deathBlackScreenImage, 0f);
    }
    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
