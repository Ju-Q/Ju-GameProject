using StarterAssets;
using UnityEngine;

public class PlayerDetectionZoneWithAnimCheck : MonoBehaviour
{
    [Header("Enemy & Detection Settings")]
    public EnemyAI enemyAI;
    public float detectionRange = 10f; // 检测距离

    [Header("Animator State Check")]
    public Animator enemyAnimator; // 敌人的 Animator
    public string[] forceDetectionStates; // 需要强制发现的 Animator 状态名字

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller == null) return;

            float distance = Vector3.Distance(enemyAI.transform.position, other.transform.position);

            // 先判断 Animator 是否在指定状态
            if (IsInForceDetectionState())
            {
                if (distance < detectionRange)
                {
                    enemyAI.TryForceDetection();
                    return; // 直接返回，不需要再判断蹲下
                }
            }

            // 如果不在特定状态，则仍按原来的逻辑判断蹲下
            if (!controller.isCrouching && distance < detectionRange)
            {
                enemyAI.TryForceDetection();
            }
        }
    }

    // 检查 Animator 是否处于 forceDetectionStates 中的状态
    private bool IsInForceDetectionState()
    {
        if (enemyAnimator == null || forceDetectionStates.Length == 0) return false;

        AnimatorStateInfo currentState = enemyAnimator.GetCurrentAnimatorStateInfo(0); // 假设是Base Layer 0
        foreach (string stateName in forceDetectionStates)
        {
            if (currentState.IsName(stateName))
            {
                return true;
            }
        }
        return false;
    }
}
