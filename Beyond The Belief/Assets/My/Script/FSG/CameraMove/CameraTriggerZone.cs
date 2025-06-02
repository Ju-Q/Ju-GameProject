using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public Transform cameraTargetPoint;        // 指定的机位
    public float transitionSpeed = 2.0f;       // 每个Trigger Box可设置不同速度

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraSwitcher switcher = other.GetComponentInChildren<CameraSwitcher>();
            if (switcher != null)
            {
                switcher.SwitchCameraTo(cameraTargetPoint, transitionSpeed);
            }
        }
    }
}
