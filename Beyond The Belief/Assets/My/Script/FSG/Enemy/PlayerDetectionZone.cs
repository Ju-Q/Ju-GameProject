using StarterAssets;
using UnityEngine;

public class PlayerDetectionZone : MonoBehaviour
{
    public EnemyAI enemyAI;
    public float detectionRange = 10f; // 设置你想要的检测距离

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller != null && !controller.isCrouching)
            {
                float distance = Vector3.Distance(enemyAI.transform.position, other.transform.position);
                if (distance < detectionRange)
                {
                    enemyAI.TryForceDetection(); // 强制发现
                }
            }
        }
    }
}
