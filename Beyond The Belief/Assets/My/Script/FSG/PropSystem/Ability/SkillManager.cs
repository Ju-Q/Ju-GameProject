using StarterAssets;
using UnityEngine;
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

    private float preChargeTimer = 0f;
    private float preChargeDelay = 0.5f;
    private bool isPreCharging = false;

    [Header("Skill VFX Settings")]
    public GameObject skillVFX;
    public float vfxDeactivateDelay = 2f;

    [Header("Skill Trigger Settings")]
    public GameObject skillTriggerObject;
    public float triggerExpandDuration = 1f;
    public float triggerStartRadius = 0.1f;
    public float triggerEndRadius = 3f;

    [Header("Skill Audio Settings")]
    public AudioSource audioSource;          // 用于播放音效的 AudioSource
    public AudioClip chargeLoopClip;         // 蓄能音效 (循环)
    public AudioClip releaseClip;            // 释放音效
    public AudioClip interruptClip;          // 中断音效

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
            return;

        if (isSkillPlaying) return;

        if (inputs.skillHold)
        {
            if (playerController != null && playerController.isCrouching)
                return; // 蹲下时不能释放技能

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
                        isPreCharging = false;
                        isCharging = true;
                        chargeTimer = 0f;
                        isChargeComplete = false;

                        // 播放蓄能音效（循环）
                        if (audioSource != null && chargeLoopClip != null)
                        {
                            audioSource.clip = chargeLoopClip;
                            audioSource.loop = true;
                            audioSource.Play();
                        }

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

                        // 停止蓄能音效
                        StopChargeSFX();

                        // 播放释放音效
                        if (audioSource != null && releaseClip != null)
                            audioSource.PlayOneShot(releaseClip);

                        StartCoroutine(PlayFullSkillAnimationFrom(chargeTimer));
                    }
                }
            }
        }
        else if (isCharging && !isChargeComplete)
        {
            Debug.Log("提前松手，反向播放动画");
            isCharging = false;

            // 停止蓄能音效
            StopChargeSFX();

            // 播放中断音效
            if (audioSource != null && interruptClip != null)
                audioSource.PlayOneShot(interruptClip);

            StartCoroutine(ReverseSkillAnimation());
            if (skillVFX != null)
                skillVFX.SetActive(false);
        }
    }

    private void StopChargeSFX()
    {
        if (audioSource != null && audioSource.clip == chargeLoopClip)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }
    }

    private IEnumerator PlayFullSkillAnimationFrom(float currentTime)
    {
        isSkillPlaying = true;

        if (skillTriggerObject != null)
            StartCoroutine(ExpandSkillTrigger());

        animator.speed = 1f;
        animator.Play(skillAnimationStateName, 0, currentTime / skillAnimLength);

        yield return new WaitForSeconds(skillAnimLength - currentTime);

        animator.Play("Idle Walk Run Blend");
        controllerToDisable.enabled = true;
        isSkillPlaying = false;

        if (skillVFX != null)
            StartCoroutine(DeactivateVFXAfterDelay(vfxDeactivateDelay));
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

            StopChargeSFX();

            if (audioSource != null && releaseClip != null)
                audioSource.PlayOneShot(releaseClip);

            StartCoroutine(PlayFullSkillAnimationFrom(chargeTimer));
        }
    }

    private IEnumerator DeactivateVFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skillVFX != null)
            skillVFX.SetActive(false);
    }

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

        skillTriggerObject.SetActive(false);
    }
}
