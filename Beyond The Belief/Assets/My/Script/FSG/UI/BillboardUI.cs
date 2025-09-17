using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    public Transform target;   // 箱子（跟随目标）
    public Vector3 offset = new Vector3(0, 2f, 0); // UI 相对箱子的位置偏移（浮在上方）

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 位置：始终跟随箱子上方
        transform.position = target.position + offset;

        // 朝向：始终面向相机
        transform.forward = cam.forward;
    }
}