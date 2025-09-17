using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAndPlaySound : MonoBehaviour
{
    [Header("检测目标")]
    public string playerTag = "Player";          // 主角Tag
    public Animator playerAnimator;              // 主角的Animator（用来检测状态）

    [Header("状态条件")]
    public List<string> validStates;             // 允许触发的状态名列表
    public int animatorLayer = 0;                // 检查的Animator Layer

    [Header("触发条件")]
    public KeyCode triggerKey = KeyCode.F;       // 触发按键
    public float waitTime = 2f;                  // 等待时间
    public AudioClip soundToPlay;                // 指定音效
    public AudioSource audioSource;              // 音效播放器

    [Header("触发限制")]
    public bool onlyOnce = false;                // 是否只能触发一次
    public float cooldownTime = 0f;              // 冷却时间（秒）

    [Header("额外动作")]
    public Animator targetAnimator;              // 额外要触发的Animator
    public string triggerName;                   // 要激活的Trigger名（可为空）

    private bool isPlayerInside = false;
    private bool hasTriggered = false;
    private float lastTriggerTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            if (playerAnimator == null)
            {
                playerAnimator = other.GetComponent<Animator>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
        }
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (onlyOnce && hasTriggered) return; // 已触发且只允许一次
        if (Time.time < lastTriggerTime + cooldownTime) return; // 冷却中

        if (Input.GetKeyDown(triggerKey))
        {
            if (IsInValidState())
            {
                StartCoroutine(WaitAndTrigger());
            }
        }
    }

    private bool IsInValidState()
    {
        if (playerAnimator == null) return false;

        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(animatorLayer);
        foreach (string stateName in validStates)
        {
            if (stateInfo.IsName(stateName))
                return true;
        }
        return false;
    }

    private IEnumerator WaitAndTrigger()
    {
        lastTriggerTime = Time.time;   // 记录触发时间
        if (onlyOnce) hasTriggered = true;

        yield return new WaitForSeconds(waitTime);


        // 触发Animator Trigger
        if (targetAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            targetAnimator.SetTrigger(triggerName);
            // 播放音效
            if (audioSource != null && soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
    }
}
