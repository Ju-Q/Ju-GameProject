using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Transform cameraHolder;

    private Transform targetPoint;
    private float transitionSpeed;
    private bool isTransitioning = false;

    public void SwitchCameraTo(Transform newTargetPoint, float newTransitionSpeed)
    {
        targetPoint = newTargetPoint;
        transitionSpeed = newTransitionSpeed;
        isTransitioning = true;
    }

    void LateUpdate()
    {
        if (isTransitioning && targetPoint != null)
        {
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetPoint.localPosition, Time.deltaTime * transitionSpeed);
            cameraHolder.localRotation = Quaternion.Lerp(cameraHolder.localRotation, targetPoint.localRotation, Time.deltaTime * transitionSpeed);

            if (Vector3.Distance(cameraHolder.localPosition, targetPoint.localPosition) < 0.01f &&
                Quaternion.Angle(cameraHolder.localRotation, targetPoint.localRotation) < 0.5f)
            {
                cameraHolder.localPosition = targetPoint.localPosition;
                cameraHolder.localRotation = targetPoint.localRotation;
                isTransitioning = false;
            }
        }
    }
}
