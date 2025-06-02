using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public Transform cameraTargetPoint;        // ָ���Ļ�λ
    public float transitionSpeed = 2.0f;       // ÿ��Trigger Box�����ò�ͬ�ٶ�

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
