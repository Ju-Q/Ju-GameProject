using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public CameraManager cameraManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraManager.SwitchToFixedView();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraManager.SwitchToMainView();
        }
    }
}