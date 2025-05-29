using UnityEngine;

public class CameraZoneController : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform targetTransform;   // ��д��λ
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
            Debug.LogError("���� CameraZoneController ��ָ�� Player Transform");
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
                    // �ӵ�ǰλ��ƽ�����ɵ���д��λ
                    cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetTransform.position, t);
                    cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetTransform.rotation, t);
                    break;

                case CameraState.ToPlayerRelative:
                    // �뿪ʱ������ʵʱ����Ŀ��λ�ã�����λ��+ƫ�ƣ�
                    Vector3 desiredPos = playerTransform.TransformPoint(relativePositionOffset);
                    Quaternion desiredRot = playerTransform.rotation * relativeRotationOffset;

                    // ƽ���ӵ�ǰλ�ù��ɵ��������λ��
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
            // ������ɺ󣬳����������λ�ú���ת
            cameraTransform.position = playerTransform.TransformPoint(relativePositionOffset);
            cameraTransform.rotation = playerTransform.rotation * relativeRotationOffset;
        }
    }

    public void EnterZone()
    {
        // ����ʱ��¼��ǰ���ƫ��
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
