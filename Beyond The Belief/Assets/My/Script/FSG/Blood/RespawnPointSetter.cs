using UnityEngine;

public class RespawnPointSetter : MonoBehaviour
{
    public Transform respawnPoint; // Õœ»Î∏¥ªÓµ„ Transform

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RespawnManager.Instance.SetCurrentRespawnPoint(respawnPoint);
        }
    }
}
