using UnityEngine;

public class SmoothCameraSwitcher : MonoBehaviour
{
    [Header("摄像机设置")]
    public Camera mainCamera;
    public Camera tradeCamera;
    public float transitionSpeed = 5f;

    [Header("触发区域设置")]
    public Collider triggerZone;

    private bool inTradeZone = false;
    private float transitionProgress = 0f;

    private void Start()
    {
        mainCamera.enabled = true;
        tradeCamera.enabled = false;
    }

    private void Update()
    {
        if (inTradeZone)
        {
            // 平滑过渡到交易摄像机
            transitionProgress = Mathf.Clamp01(transitionProgress + Time.deltaTime * transitionSpeed);
        }
        else
        {
            // 平滑过渡回主摄像机
            transitionProgress = Mathf.Clamp01(transitionProgress - Time.deltaTime * transitionSpeed);
        }

        // 根据过渡进度混合摄像机
        if (transitionProgress > 0)
        {
            tradeCamera.enabled = true;
            mainCamera.enabled = transitionProgress < 1;

            // 这里可以添加更复杂的过渡效果，如插值位置、旋转等
        }
        else
        {
            mainCamera.enabled = true;
            tradeCamera.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inTradeZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inTradeZone = false;
        }
    }
}