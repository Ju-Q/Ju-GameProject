using StarterAssets;
using UnityEngine;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public Animator animator;                         // 控制技能动画的 Animator
    public StarterAssetsInputs inputs;                // 管理玩家输入的脚本引用
    public MonoBehaviour controllerToDisable;         // 技能释放期间要禁用的控制器（如 ThirdPersonController）

    [Header("Skill Animation Settings")]
    public string skillAnimationStateName = "SkillChargeRelease"; // 技能动画的状态名称
    public float skillAnimLength = 2.17f;             // 技能动画总时长
    public float chargeThreshold = 1.15f;             // 技能蓄力完成所需时间阈值

    private float chargeTimer = 0f;                   // 当前蓄力计时器
    private bool isCharging = false;                  // 是否正在蓄力
    private bool isSkillPlaying = false;              // 是否正在播放技能动画
    private bool isChargeComplete = false;            // 是否已经完成蓄力

    private float preChargeTimer = 0f;                // 记录预蓄力时间
    private float preChargeDelay = 0.5f;              // 容错时间：误触0.5秒内不触发蓄力
    private bool isPreCharging = false;               // 是否在预蓄力阶段

    [Header("Skill VFX Settings")]
    public GameObject skillVFX;                       // 技能特效物体
    public float vfxDeactivateDelay = 2f;             // 动画播放完成后延迟关闭特效的时间

    [Header("Skill Trigger Settings")]
    public GameObject skillTriggerObject;             // 技能 Trigger 对象（需带 SphereCollider，isTrigger = true）
    public float triggerExpandDuration = 1f;          // Trigger 扩大所用时间
    public float triggerStartRadius = 0.1f;           // 起始半径
    public float triggerEndRadius = 3f;               // 最终半径

    private ItemPickupManager itemPickupManager;
    private ThirdPersonController playerController;
    void Start()
    {
        itemPickupManager = GetComponent<ItemPickupManager>();
        playerController = GetComponent<ThirdPersonController>(); 
    }

    private void Update()
    {

        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (isSkillPlaying) return;

        if (inputs.skillHold)
        {
            if (playerController != null && playerController.isCrouching)
            {
                // 蹲下时不能释放技能
                return;
            }

            if (SkillPointManager.Instance != null &&
                SkillPointManager.Instance.currentSkillPoints > 0 &&
                itemPickupManager != null && itemPickupManager.propACount > 0)
            {
                if (!isCharging && !isPreCharging)
                {
                    isPreCharging = true;
                    preChargeTimer = 0f;
                }

                if (isPreCharging && !isCharging)
                {
                    preChargeTimer += Time.deltaTime;

                    if (preChargeTimer >= preChargeDelay)
                    {
                        Debug.Log("开始蓄力");

                        isPreCharging = false;
                        isCharging = true;
                        chargeTimer = 0f;
                        isChargeComplete = false;

                        if (skillVFX != null)
                        {
                            skillVFX.SetActive(false);
                            skillVFX.SetActive(true);
                            skillVFX.GetComponent<ParticleSystem>()?.Play();
                        }
                           
                        controllerToDisable.enabled = false;
                        animator.speed = 1f;
                        animator.Play(skillAnimationStateName, 0, 0f);
                    }
                }

                if (isCharging)
                {
                    chargeTimer += Time.deltaTime;

                    float normTime = Mathf.Clamp01(chargeTimer / skillAnimLength);
                    animator.Play(skillAnimationStateName, 0, normTime);

                    if (!isChargeComplete && chargeTimer >= chargeThreshold)
                    {
                        Debug.Log("蓄力完成，自动释放技能");
                        isChargeComplete = true;
                        isCharging = false;
                        StartCoroutine(PlayFullSkillAnimationFrom(chargeTimer));
                    }
                }
            }
        }
        else if (isCharging && !isChargeComplete)
        {
            Debug.Log("提前松手，反向播放动画");
            isCharging = false;
            StartCoroutine(ReverseSkillAnimation());
            if(skillVFX != null)
            {
                skillVFX.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 播放技能剩余动画，并在同时启用扩大范围 Trigger
    /// </summary>
    private IEnumerator PlayFullSkillAnimationFrom(float currentTime)
    {
        isSkillPlaying = true;

        // ✅ 同步启用技能 Trigger 范围扩大
        if (skillTriggerObject != null)
        {
            StartCoroutine(ExpandSkillTrigger());
        }

        animator.speed = 1f;
        animator.Play(skillAnimationStateName, 0, currentTime / skillAnimLength);

        yield return new WaitForSeconds(skillAnimLength - currentTime);

        // 动画结束后回到 Idle 状态并恢复控制
        animator.Play("Idle Walk Run Blend");
        controllerToDisable.enabled = true;
        isSkillPlaying = false;

        if (skillVFX != null)
            StartCoroutine(DeactivateVFXAfterDelay(vfxDeactivateDelay));
    }

    /// <summary>
    /// 技能未充满时，反向播放动画回到起始
    /// </summary>
    private IEnumerator ReverseSkillAnimation()
    {
        isSkillPlaying = true;
        float t = chargeTimer;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            float normTime = Mathf.Clamp01(t / skillAnimLength);
            animator.Play(skillAnimationStateName, 0, normTime);
            yield return null;
        }

        animator.Play("Idle Walk Run Blend");
        controllerToDisable.enabled = true;
        isSkillPlaying = false;
    }

    /// <summary>
    /// 强制释放技能（用于外部调用）
    /// </summary>
    public void ForceReleaseSkill()
    {
        if (isCharging && !isSkillPlaying && !isChargeComplete)
        {
            Debug.Log("手动触发技能释放");
            isChargeComplete = true;
            isCharging = false;
            StartCoroutine(PlayFullSkillAnimationFrom(chargeTimer));
        }
    }

    /// <summary>
    /// 延迟关闭技能特效
    /// </summary>
    private IEnumerator DeactivateVFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skillVFX != null)
            skillVFX.SetActive(false);
    }

    /// <summary>
    /// 技能释放判定范围（Trigger）从小扩大到大
    /// </summary>
    private IEnumerator ExpandSkillTrigger()
    {
        SphereCollider trigger = skillTriggerObject.GetComponent<SphereCollider>();
        if (trigger == null)
        {
            Debug.LogWarning("技能Trigger对象上缺少SphereCollider");
            yield break;
        }

        skillTriggerObject.SetActive(true);
        trigger.enabled = true;
        trigger.radius = triggerStartRadius;

        float timer = 0f;
        while (timer < triggerExpandDuration)
        {
            timer += Time.deltaTime;
            float t = timer / triggerExpandDuration;
            trigger.radius = Mathf.Lerp(triggerStartRadius, triggerEndRadius, t);
            yield return null;
        }

        yield return null;
        skillTriggerObject.SetActive(false);
    }
}
