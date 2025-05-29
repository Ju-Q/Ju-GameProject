using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public Transform player;
    public Camera mainCamera;
    public Camera fixedCamera;
    public float transitionDuration = 2.0f;

    private Vector3 mainCameraOffset;
    private Quaternion mainCameraInitialRotation;
    private bool isInFixedView = false;
    private Coroutine transitionCoroutine;

    void Start()
    {
        // ��¼��������������ҵĳ�ʼƫ�ƺ���ת
        mainCameraOffset = mainCamera.transform.position - player.position;
        mainCameraInitialRotation = mainCamera.transform.rotation;

        // ��ʼ״̬����
        mainCamera.gameObject.SetActive(true);
        mainCamera.enabled = true;
        fixedCamera.gameObject.SetActive(false);
        fixedCamera.enabled = false;
    }

    void Update()
    {
        // �����������Ӧ���ڵ�λ�ú���ת
        Vector3 desiredMainCamPos = player.position + mainCameraOffset;
        Quaternion desiredMainCamRot = mainCameraInitialRotation;

        // ������ڹ������Ҳ��ڹ̶��ӽǣ�ֱ�Ӹ����������
        if (!isInFixedView && transitionCoroutine == null)
        {
            mainCamera.transform.position = desiredMainCamPos;
            mainCamera.transform.rotation = desiredMainCamRot;
        }
    }

    public void SwitchToFixedView()
    {
        if (isInFixedView) return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToFixed());
        isInFixedView = true;
    }

    public void SwitchToMainView()
    {
        if (!isInFixedView) return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToMain());
        isInFixedView = false;
    }

    private IEnumerator TransitionToFixed()
    {
        float elapsedTime = 0f;

        // ����̶������
        fixedCamera.gameObject.SetActive(true);
        fixedCamera.enabled = false; // �Ȳ�������Ⱦ

        // ��ȡ��ʼλ�ú���ת
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = fixedCamera.transform.position;
        Quaternion endRot = fixedCamera.transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            // ��ֵλ�ú���ת
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // ȷ������λ�ú���ת׼ȷ
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        // �л������
        mainCamera.enabled = false;
        fixedCamera.enabled = true;

        transitionCoroutine = null;
    }

    private IEnumerator TransitionToMain()
    {
        float elapsedTime = 0f;

        // �����������Ӧ���ڵ�λ�ú���ת
        Vector3 desiredMainCamPos = player.position + mainCameraOffset;
        Quaternion desiredMainCamRot = mainCameraInitialRotation;

        // ��ȡ�̶��������ǰ״̬��Ϊ�������
        Vector3 startPos = fixedCamera.transform.position;
        Quaternion startRot = fixedCamera.transform.rotation;

        // ȷ��������������ʱ����Ⱦ
        mainCamera.gameObject.SetActive(true);
        mainCamera.enabled = false;

        // ��ʼ����ǰ����������������̶��������λ��
        mainCamera.transform.position = startPos;
        mainCamera.transform.rotation = startRot;

        // �����������������������Ⱦ��
        mainCamera.enabled = false;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            // ��ֵλ�ú���ת���������Ӧ�õ�λ��
            mainCamera.transform.position = Vector3.Lerp(startPos, desiredMainCamPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, desiredMainCamRot, t);

            // ȷ��������һ�����������Ⱦ
            if (!fixedCamera.enabled && !mainCamera.enabled && elapsedTime > 0.1f)
            {
                mainCamera.enabled = true;
            }

            yield return null;
        }

        // ȷ������λ�ú���ת׼ȷ
        mainCamera.transform.position = desiredMainCamPos;
        mainCamera.transform.rotation = desiredMainCamRot;

        // �л������
        mainCamera.enabled = true;
        fixedCamera.enabled = false;
        fixedCamera.gameObject.SetActive(false);

        transitionCoroutine = null;
    }
}