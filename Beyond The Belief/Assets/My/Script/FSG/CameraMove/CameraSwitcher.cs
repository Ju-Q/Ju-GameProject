using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Transform cameraHolder;

    private Transform targetPoint;
    private float transitionSpeed;
    private bool isTransitioning = false;

    public Transform CurrentTarget { get; private set; }   // 当前摄像机目标

    public void SwitchCameraTo(Transform newTargetPoint, float newTransitionSpeed)
    {
        // 每次切换更新当前目标与参数
        CurrentTarget = newTargetPoint;
        targetPoint = newTargetPoint;
        transitionSpeed = newTransitionSpeed;
        isTransitioning = true;
    }

    void LateUpdate()
    {
        if (targetPoint == null || cameraHolder == null) return;

        if (isTransitioning)
        {
            // 平滑插值移动到目标位置
            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                targetPoint.localPosition,
                Time.deltaTime * transitionSpeed);

            cameraHolder.localRotation = Quaternion.Lerp(
                cameraHolder.localRotation,
                targetPoint.localRotation,
                Time.deltaTime * transitionSpeed);

            // 当位置 & 角度接近目标时，直接对齐并结束过渡
            if (Vector3.Distance(cameraHolder.localPosition, targetPoint.localPosition) < 0.01f &&
                Quaternion.Angle(cameraHolder.localRotation, targetPoint.localRotation) < 0.5f)
            {
                cameraHolder.localPosition = targetPoint.localPosition;
                cameraHolder.localRotation = targetPoint.localRotation;
                isTransitioning = false;
            }
        }
        else
        {
            // 如果还在目标点，但外部改了targetPoint，继续对齐
            if (cameraHolder.localPosition != targetPoint.localPosition ||
                cameraHolder.localRotation != targetPoint.localRotation)
            {
                cameraHolder.localPosition = Vector3.Lerp(
                    cameraHolder.localPosition,
                    targetPoint.localPosition,
                    Time.deltaTime * transitionSpeed);

                cameraHolder.localRotation = Quaternion.Lerp(
                    cameraHolder.localRotation,
                    targetPoint.localRotation,
                    Time.deltaTime * transitionSpeed);
            }
        }
    }
}
