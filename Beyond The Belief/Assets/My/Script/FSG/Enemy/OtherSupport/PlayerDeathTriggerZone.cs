using UnityEngine;

public class PlayerDeathTriggerZone : MonoBehaviour
{
    [Header("敌人控制")]
    public EnemyAI enemyAI; // 拖拽敌人对象的 EnemyAI 脚本
    [Header("触发区域")]
    public Collider triggerZone; // 拖拽你想检测的区域（必须勾选 isTrigger）
    [Header("目标动画")]
    public Animator targetAnimator; // 拖拽要触发动画的 Animator
    public string triggerName = "SpecialEvent"; // Animator 的 trigger 名字

    private bool playerHasEnteredZone = false; // 玩家是否进入过区域
    private bool hasTriggered = false;         // 是否已经触发过逻辑

    void Update()
    {
        if (enemyAI == null || enemyAI.Controller == null || triggerZone == null || hasTriggered)
            return;

        if (!playerHasEnteredZone && enemyAI.Controller.isDead)
        {
            TriggerAnimation();
        }
    }

    private void TriggerAnimation()
    {
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

    // 可用于重置（例如重开关卡）
    public void ResetTriggerState()
    {
        hasTriggered = false;
        playerHasEnteredZone = false;
    }
}
