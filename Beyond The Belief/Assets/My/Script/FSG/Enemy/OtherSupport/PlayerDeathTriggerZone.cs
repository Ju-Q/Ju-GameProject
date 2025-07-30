using UnityEngine;
using System.Collections;

public class PlayerDeathTriggerZone : MonoBehaviour
{
    [Header("敌人控制（持有玩家Controller引用）")]
    public EnemyAI enemyAI;

    [Header("触发区域")]
    public Collider triggerZone;

    [Header("目标动画")]
    public Animator targetAnimator;
    public string triggerName = "SpecialEvent";

    [Header("触发延迟")]
    public float triggerDelay = 1f; // 延迟秒数

    private bool playerHasEnteredZone = false;
    private bool isTriggerCoroutineRunning = false; // 防止重复启动协程

    void Update()
    {
        if (enemyAI == null || enemyAI.Controller == null || triggerZone == null)
            return;

        // 如果玩家没进入区域且玩家死亡，触发延迟动画
        if (!playerHasEnteredZone && enemyAI.Controller.isDead && !isTriggerCoroutineRunning)
        {
            StartCoroutine(TriggerAnimation());
        }

        // 玩家复活后允许重新触发
        if (!enemyAI.Controller.isDead)
        {
            isTriggerCoroutineRunning = false;
        }
    }

    private IEnumerator TriggerAnimation()
    {
        isTriggerCoroutineRunning = true;
        yield return new WaitForSeconds(triggerDelay);

        if (targetAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            targetAnimator.SetTrigger(triggerName);
            yield return null; // 等一帧再重置，确保触发成功
            targetAnimator.ResetTrigger(triggerName);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHasEnteredZone = true;
            Debug.Log("玩家已进入指定区域，标记为已进入。");
        }
    }

    public void ResetTriggerState()
    {
        playerHasEnteredZone = false;
        isTriggerCoroutineRunning = false;
    }
}
