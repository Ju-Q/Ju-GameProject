using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    public BossFollowPlayer bossFollow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossFollow?.ActivateBossFollow();
            gameObject.SetActive(false); // 触发一次后禁用 Trigger
        }
    }
}
