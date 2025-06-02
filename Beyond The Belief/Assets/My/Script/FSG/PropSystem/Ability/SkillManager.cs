using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public Animator animator;
    public StarterAssetsInputs inputs;
    public MonoBehaviour controllerToDisable;

    [Header("Skill Animation Settings")]
    public string skillAnimationStateName = "SkillChargeRelease";
    public float skillAnimLength = 2.17f;
    public float chargeThreshold = 1.15f;

    private float chargeTimer = 0f;
    private bool isCharging = false;
    private bool isSkillPlaying = false;
    private bool isChargeComplete = false;

    private void Update()
    {
        if (isSkillPlaying) return;

        if (inputs.skillHold)
        {
            if (!isCharging)
            {
                Debug.Log("开始蓄力");
                isCharging = true;
                chargeTimer = 0f;
                isChargeComplete = false;

                controllerToDisable.enabled = false;
                animator.speed = 1f;
                animator.Play(skillAnimationStateName, 0, 0f);
            }

            chargeTimer += Time.deltaTime;

            float normTime = Mathf.Clamp01(chargeTimer / skillAnimLength);
            animator.Play(skillAnimationStateName, 0, normTime);

            // 自动触发技能释放逻辑（不等待松手）
            if (!isChargeComplete && chargeTimer >= chargeThreshold)
            {
                Debug.Log("蓄力完成，自动释放技能");
                isChargeComplete = true;
                StartCoroutine(PlayFullSkillAnimationFrom(chargeTimer));
                isCharging = false;
            }
        }
        else if (isCharging && !isChargeComplete)
        {
            Debug.Log("提前松手，反向播放动画");
            isCharging = false;
            StartCoroutine(ReverseSkillAnimation());
        }
    }

    private IEnumerator PlayFullSkillAnimationFrom(float currentTime)
    {
        isSkillPlaying = true;
        animator.speed = 1f;
        animator.Play(skillAnimationStateName, 0, currentTime / skillAnimLength);

        yield return new WaitForSeconds(skillAnimLength - currentTime);

        animator.Play("Idle"); // 或你实际的 idle 状态名
        controllerToDisable.enabled = true;
        isSkillPlaying = false;
    }

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
}
