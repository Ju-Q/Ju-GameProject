using StarterAssets;
using UnityEngine;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public Animator animator; // 控制技能动画的 Animator
    public StarterAssetsInputs inputs; // 管理玩家输入的脚本引用
    public MonoBehaviour controllerToDisable; // 技能释放期间要禁用的控制器（例如角色控制器）

    [Header("Skill Animation Settings")]
    public string skillAnimationStateName = "SkillChargeRelease"; // 技能动画的状态名称
    public float skillAnimLength = 2.17f; // 技能动画总时长
    public float chargeThreshold = 1.15f; // 技能蓄力完成所需时间阈值

    private float chargeTimer = 0f; // 当前蓄力计时器
    private bool isCharging = false; // 是否正在蓄力
    private bool isSkillPlaying = false; // 是否正在播放技能动画
    private bool isChargeComplete = false; // 是否已经完成蓄力

    private float preChargeTimer = 0f;           // 记录预蓄力时间
    private float preChargeDelay = 0.5f;         // 容错时间：误触0.5秒内不触发蓄力
    private bool isPreCharging = false;          // 是否在预蓄力阶段

    [Header("Skill VFX Settings")]
    public GameObject skillVFX;              // 技能特效物体
    public float vfxDeactivateDelay = 2f;    // 动画播放完成后延迟关闭特效的时间

    private ItemPickupManager itemPickupManager;

    void Start()
    {
        // 获取 ItemPickupManager 脚本引用
        itemPickupManager = GetComponent<ItemPickupManager>();
    }

    private void Update()
    {
        // 如果正在播放技能动画，跳过更新逻辑
        if (isSkillPlaying) return;

        // 如果玩家正在按住技能键
        if (inputs.skillHold)
        {

            if (itemPickupManager != null && itemPickupManager.propACount > 0)
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
                        // 容错时间结束，开始正式蓄力
                        Debug.Log("开始蓄力");


                        isPreCharging = false;
                        isCharging = true;
                        chargeTimer = 0f;
                        isChargeComplete = false;

                        if (skillVFX != null)
                            skillVFX.SetActive(true);

                        controllerToDisable.enabled = false;
                        animator.speed = 1f;
                        animator.Play(skillAnimationStateName, 0, 0f);
                    }
                }


                if (isCharging)
                {
                    // 累加蓄力时间
                    chargeTimer += Time.deltaTime;

                    // 按当前蓄力时间设置动画播放进度
                    float normTime = Mathf.Clamp01(chargeTimer / skillAnimLength);
                    animator.Play(skillAnimationStateName, 0, normTime);

                    // 如果达到蓄力阈值，自动释放技能
                    if (!isChargeComplete && chargeTimer >= chargeThreshold)
                    {
                        Debug.Log("蓄力完成，自动释放技能");
                        isChargeComplete = true;
                        StartCoroutine(PlayFullSkillAnimationFrom(chargeTimer)); // 从当前进度继续播放完整动画
                        isCharging = false;
                    }
                }
            }
        }
        // 如果提前松开技能键但未完成蓄力，反向播放动画
        else if (isCharging && !isChargeComplete)
        {
            Debug.Log("提前松手，反向播放动画");
            isCharging = false;
            StartCoroutine(ReverseSkillAnimation()); // 播放反向动画
        }
    }

    // 协程：从指定时间点开始播放剩下的完整技能动画
    private IEnumerator PlayFullSkillAnimationFrom(float currentTime)
    {
        isSkillPlaying = true;
        animator.speed = 1f;
        animator.Play(skillAnimationStateName, 0, currentTime / skillAnimLength);

        // 等待剩余动画时长播放完成
        yield return new WaitForSeconds(skillAnimLength - currentTime);

        // 动画结束后回到 Idle 状态，恢复控制
        animator.Play("Idle Walk Run Blend"); // 或替换为你的 idle 动画状态名
        controllerToDisable.enabled = true;
        isSkillPlaying = false;

        if (skillVFX != null)
            StartCoroutine(DeactivateVFXAfterDelay(vfxDeactivateDelay));


    }

    // 协程：反向播放动画至起点（松手时未蓄满）
    private IEnumerator ReverseSkillAnimation()
    {
        isSkillPlaying = true;
        float t = chargeTimer;

        // 倒退播放动画
        while (t > 0f)
        {
            t -= Time.deltaTime;
            float normTime = Mathf.Clamp01(t / skillAnimLength);
            animator.Play(skillAnimationStateName, 0, normTime);
            yield return null;
        }

        // 回到普通状态动画，恢复控制
        animator.Play("Idle Walk Run Blend"); // 替换为你的实际 idle 动画名
        controllerToDisable.enabled = true;
        isSkillPlaying = false;
    }

    // 外部调用：强制释放技能（例如被某些事件触发）
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

    private IEnumerator DeactivateVFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skillVFX != null)
            skillVFX.SetActive(false);
    }


}
