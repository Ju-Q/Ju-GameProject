using UnityEngine;
using System.Collections;

public class PlayerDeathTriggerZone : MonoBehaviour
{
    [Header("敌人控制")]
    public EnemyAI enemyAI;

    [Header("触发区域")]
    public Collider triggerZone;

    [Header("目标动画")]
    public Animator targetAnimator;
    public string triggerName = "SpecialEvent";

    [Header("触发延迟")]
    public float triggerDelay = 1f; // 延迟秒数

    private bool playerHasEnteredZone = false;
    private bool hasTriggered = false;

    void Update()
    {
        if (enemyAI == null || enemyAI.Controller == null || triggerZone == null || hasTriggered)
            return;

        if (!playerHasEnteredZone && enemyAI.Controller.isDead)
        {
            StartCoroutine(TriggerAnimation());
        }
    }

    private IEnumerator TriggerAnimation()
    {
        yield return new WaitForSeconds(triggerDelay); // 延迟

        if (targetAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            targetAnimator.SetTrigger(triggerName);
            Debug.Log("触发动画 Trigger：" + triggerName);
        }

        hasTriggered = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other == triggerZone)
        {
            playerHasEnteredZone = true;
            Debug.Log("玩家已进入指定区域，标记为已进入。");
        }
    }

    public void ResetTriggerState()
    {
        hasTriggered = false;
        playerHasEnteredZone = false;
    }
}
