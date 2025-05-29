using UnityEngine;

public class CameraZoneController : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform targetTransform;   // 特写机位
    public Transform playerTransform;
    public float transitionDuration = 1.5f;

    private Vector3 relativePositionOffset;
    private Quaternion relativeRotationOffset;

    private float transitionTimer = 0f;
    private bool isTransitioning = false;

    private enum CameraState { Idle, ToTarget, ToPlayerRelative }
    private CameraState cameraState = CameraState.Idle;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (playerTransform == null)
            Debug.LogError("请在 CameraZoneController 中指定 Player Transform");
    }

    void Update()
    {
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);

            switch (cameraState)
            {
                case CameraState.ToTarget:
                    // 从当前位置平滑过渡到特写机位
                    cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetTransform.position, t);
                    cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetTransform.rotation, t);
                    break;

                case CameraState.ToPlayerRelative:
                    // 离开时，持续实时计算目标位置（主角位置+偏移）
                    Vector3 desiredPos = playerTransform.TransformPoint(relativePositionOffset);
                    Quaternion desiredRot = playerTransform.rotation * relativeRotationOffset;

                    // 平滑从当前位置过渡到主角相对位置
                    cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPos, t);
                    cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, desiredRot, t);
                    break;
            }

            if (t >= 1f)
            {
                isTransitioning = false;
                cameraState = CameraState.Idle;
            }
        }
        else if (cameraState == CameraState.ToPlayerRelative)
        {
            // 过渡完成后，持续保持相对位置和旋转
            cameraTransform.position = playerTransform.TransformPoint(relativePositionOffset);
            cameraTransform.rotation = playerTransform.rotation * relativeRotationOffset;
        }
    }

    public void EnterZone()
    {
        // 进入时记录当前相对偏移
        relativePositionOffset = playerTransform.InverseTransformPoint(cameraTransform.position);
        relativeRotationOffset = Quaternion.Inverse(playerTransform.rotation) * cameraTransform.rotation;

        transitionTimer = 0f;
        isTransitioning = true;
        cameraState = CameraState.ToTarget;
    }

    public void ExitZone()
    {
        transitionTimer = 0f;
        isTransitioning = true;
        cameraState = CameraState.ToPlayerRelative;
    }
}
