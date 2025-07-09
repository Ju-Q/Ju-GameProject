using StarterAssets;
using UnityEngine;

public class PlayerDetectionZone : MonoBehaviour
{
    public EnemyAI enemyAI;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller != null && !controller.isCrouching)
            {
                enemyAI.TryForceDetection(); // 强制发现
            }
        }
    }
}
