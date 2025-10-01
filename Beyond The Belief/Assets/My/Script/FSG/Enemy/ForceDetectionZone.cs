using StarterAssets;
using UnityEngine;

public class ForceDetectionZone : MonoBehaviour
{
    [Header("F键延迟设置")]
    public float delayBeforeTrigger = 3f;

    [Header("关联敌人AI")]
    public EnemyAI enemyAI;

    private bool playerInside = false;
    private bool hasTriggered = false; // 当前生命周期是否触发过
    private bool isWaiting = false;
    private Transform player;
    private ThirdPersonController controller;

    [Header("关联的需要重置的OpenGate")]
    public OpenGate targetOpenGate;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            player = other.transform;
            controller = player.GetComponent<ThirdPersonController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            player = null;
            controller = null;
        }
    }

    void Update()
    {
        if (!playerInside || hasTriggered || isWaiting || controller == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(DelayedTrigger());
        }
    }

    System.Collections.IEnumerator DelayedTrigger()
    {
        isWaiting = true;
        float timer = 0f;  

        while (timer < delayBeforeTrigger)
        {
            // 如果玩家离开了区域或死亡中断
            if (!playerInside || controller.isDead)
            {
                isWaiting = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        hasTriggered = true;
        isWaiting = false;

        if (enemyAI != null)
        {
            enemyAI.TryForceDetection();
        }
    }

    // 每次主角重生都会触发 ResetTrigger()
    public void ResetTrigger()
    {
        hasTriggered = false;

        if (targetOpenGate != null)
        {
            targetOpenGate.hasConsumedSkillPoint = false;
            //Debug.Log("OpenGate 扣点状态已重置");
        }

    }
}
