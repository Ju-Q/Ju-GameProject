using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        // 让UI朝向玩家摄像机（仅绕Y轴）
        Vector3 lookDir = transform.position - mainCamera.transform.position;
        lookDir.y = 0; // 保持竖直不变（只绕Y旋转）
        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
