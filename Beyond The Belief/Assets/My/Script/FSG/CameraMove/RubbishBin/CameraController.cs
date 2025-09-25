using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("目标对象（主角）")]
    public Transform target; // 主角

    [Header("鼠标灵敏度")]
    public float sensitivityX = 150f;  // 左右灵敏度
    public float sensitivityY = 100f;  // 上下灵敏度

    [Header("限制旋转角度")]
    public float minVerticalAngle = -20f; // 最低俯角
    public float maxVerticalAngle = 30f;  // 最高仰角
    public float maxHorizontalAngle = 45f; // 左右最大偏移角度（相对于角色正面）

    [Header("相机参数")]
    public float distance = 5f;       // 距离主角的距离
    public float height = 2f;         // 高度偏移
    public float smoothTime = 0.1f;   // 平滑时间

    private float yaw;
    private float pitch;
    private Vector3 currentVelocity; // 平滑用

    void Start()
    {
        Vector3 angles = transform.localEulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        if (target != null)
        {
            transform.position = target.position - transform.forward * distance + Vector3.up * height;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        // 累加旋转
        yaw += mouseX;
        pitch -= mouseY;

        // 限制上下角度
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // 限制左右角度（相对于目标正面）
        yaw = Mathf.Clamp(yaw, -maxHorizontalAngle, maxHorizontalAngle);

        // 计算期望旋转
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 计算目标位置
        Vector3 targetPosition = target.position - rotation * Vector3.forward * distance + Vector3.up * height;

        // 平滑移动
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 相机始终看向主角
        transform.rotation = rotation;
    }
}
