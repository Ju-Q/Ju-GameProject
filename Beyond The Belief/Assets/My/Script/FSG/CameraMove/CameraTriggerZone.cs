using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public Transform cameraTargetPoint;        // 指定的机位
    public float transitionSpeed = 2.0f;       // 每个Trigger Box可设置不同速度

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraSwitcher switcher = other.GetComponentInChildren<CameraSwitcher>();
            if (switcher != null)
            {
                // 如果当前摄像机目标不是这个区域的目标，则切换到新的目标
                if (switcher.CurrentTarget != cameraTargetPoint)
                {
                    switcher.SwitchCameraTo(cameraTargetPoint, transitionSpeed);
                }
            }
        }
    }
}
