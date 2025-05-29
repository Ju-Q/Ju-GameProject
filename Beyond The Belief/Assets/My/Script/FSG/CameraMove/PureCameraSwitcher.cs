using UnityEngine;

[RequireComponent(typeof(Collider))] // ȷ���ű����ڶ�����Collider
public class PureCameraSwitcher : MonoBehaviour
{
    [Header("���������")]
    public Camera playerCamera;  // ��ҳ��������
    public Camera fixedCamera;   // �̶��ӽ������

    [Header("����������")]
    [Tooltip("�Զ���ȡ������ͬһ�����ϵ�Collider")]
    public Collider triggerCollider;

    private void Awake()
    {
        // �Զ���ȡCollider���
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        // ȷ��Collider�Ǵ�����
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        // ��ʼ�������״̬
        playerCamera.enabled = true;
        fixedCamera.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ʹ��LayerMask���ɿ��ļ�����
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("��ҽ��봥������");
            SwitchToFixedView();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("����뿪��������");
            SwitchToPlayerView();
        }
    }

    private void SwitchToFixedView()
    {
        playerCamera.enabled = false;
        fixedCamera.enabled = true;
    }

    private void SwitchToPlayerView()
    {
        playerCamera.enabled = true;
        fixedCamera.enabled = false;
    }

    // ���Թ���
    [ContextMenu("�����л����̶��ӽ�")]
    public void TestFixedView()
    {
        SwitchToFixedView();
    }

    [ContextMenu("�����л�������ӽ�")]
    public void TestPlayerView()
    {
        SwitchToPlayerView();
    }
}