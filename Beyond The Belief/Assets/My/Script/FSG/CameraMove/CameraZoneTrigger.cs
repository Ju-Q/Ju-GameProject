using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    public CameraZoneController cameraZoneController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraZoneController.EnterZone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cameraZoneController.ExitZone();
        }
    }
}
