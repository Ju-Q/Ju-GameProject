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
        // 记录主摄像机相对于玩家的初始偏移和旋转
        mainCameraOffset = mainCamera.transform.position - player.position;
        mainCameraInitialRotation = mainCamera.transform.rotation;

        // 初始状态设置
        mainCamera.gameObject.SetActive(true);
        mainCamera.enabled = true;
        fixedCamera.gameObject.SetActive(false);
        fixedCamera.enabled = false;
    }

    void Update()
    {
        // 计算主摄像机应该在的位置和旋转
        Vector3 desiredMainCamPos = player.position + mainCameraOffset;
        Quaternion desiredMainCamRot = mainCameraInitialRotation;

        // 如果不在过渡中且不在固定视角，直接更新主摄像机
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

        // 激活固定摄像机
        fixedCamera.gameObject.SetActive(true);
        fixedCamera.enabled = false; // 先不启用渲染

        // 获取初始位置和旋转
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = fixedCamera.transform.position;
        Quaternion endRot = fixedCamera.transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            // 插值位置和旋转
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // 确保最终位置和旋转准确
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        // 切换摄像机
        mainCamera.enabled = false;
        fixedCamera.enabled = true;

        transitionCoroutine = null;
    }

    private IEnumerator TransitionToMain()
    {
        float elapsedTime = 0f;

        // 计算主摄像机应该在的位置和旋转
        Vector3 desiredMainCamPos = player.position + mainCameraOffset;
        Quaternion desiredMainCamRot = mainCameraInitialRotation;

        // 获取固定摄像机当前状态作为过渡起点
        Vector3 startPos = fixedCamera.transform.position;
        Quaternion startRot = fixedCamera.transform.rotation;

        // 确保主摄像机激活但暂时不渲染
        mainCamera.gameObject.SetActive(true);
        mainCamera.enabled = false;

        // 开始过渡前先设置主摄像机到固定摄像机的位置
        mainCamera.transform.position = startPos;
        mainCamera.transform.rotation = startRot;

        // 激活主摄像机（但不启用渲染）
        mainCamera.enabled = false;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            // 插值位置和旋转到主摄像机应该的位置
            mainCamera.transform.position = Vector3.Lerp(startPos, desiredMainCamPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, desiredMainCamRot, t);

            // 确保至少有一个摄像机在渲染
            if (!fixedCamera.enabled && !mainCamera.enabled && elapsedTime > 0.1f)
            {
                mainCamera.enabled = true;
            }

            yield return null;
        }

        // 确保最终位置和旋转准确
        mainCamera.transform.position = desiredMainCamPos;
        mainCamera.transform.rotation = desiredMainCamRot;

        // 切换摄像机
        mainCamera.enabled = true;
        fixedCamera.enabled = false;
        fixedCamera.gameObject.SetActive(false);

        transitionCoroutine = null;
    }
}