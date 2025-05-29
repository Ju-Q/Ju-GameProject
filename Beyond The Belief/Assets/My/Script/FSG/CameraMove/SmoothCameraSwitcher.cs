using UnityEngine;

public class SmoothCameraSwitcher : MonoBehaviour
{
    [Header("���������")]
    public Camera mainCamera;
    public Camera tradeCamera;
    public float transitionSpeed = 5f;

    [Header("������������")]
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
            // ƽ�����ɵ����������
            transitionProgress = Mathf.Clamp01(transitionProgress + Time.deltaTime * transitionSpeed);
        }
        else
        {
            // ƽ�����ɻ��������
            transitionProgress = Mathf.Clamp01(transitionProgress - Time.deltaTime * transitionSpeed);
        }

        // ���ݹ��ɽ��Ȼ�������
        if (transitionProgress > 0)
        {
            tradeCamera.enabled = true;
            mainCamera.enabled = transitionProgress < 1;

            // ���������Ӹ����ӵĹ���Ч�������ֵλ�á���ת��
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