using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("���������")]
    public Camera mainCamera;      // �������
    public Camera tradeCamera;    // �����������
    public Transform cameraPosition; // �������λλ�ã������������ĸ�����

    [Header("��������")]
    public Collider tradeZone;    // ��������ײ��

    private bool isInTradeZone = false;

    private void Start()
    {
        // ��ʼ�������״̬
        mainCamera.enabled = true;
        tradeCamera.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInTradeZone)
        {
            // �л��������������
            mainCamera.enabled = false;
            tradeCamera.enabled = true;
            isInTradeZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInTradeZone)
        {
            // �л����������
            tradeCamera.enabled = false;
            mainCamera.enabled = true;
            isInTradeZone = false;

            // ��������������ر任
      //      ResetMainCamera();
        }
    }

    private void ResetMainCamera()
    {
        if (cameraPosition != null)
        {
            // ȷ�������ø�����
            mainCamera.transform.SetParent(cameraPosition);

            // Ӳ�����ñ任
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
            mainCamera.transform.localScale = Vector3.one;
            // ͨ���ײ�TransformAccess�ӿڣ���ҪUnityEngine.Experimentalģ�飩
            var transformAccess = mainCamera.transform;
            transformAccess.localPosition = Vector3.zero;
            transformAccess.localRotation = Quaternion.identity;
            transformAccess.localScale = Vector3.one;

            // ǿ�����¼����������
            mainCamera.SendMessage("OnTransformChanged", SendMessageOptions.DontRequireReceiver);

            // ǿ��ˢ��
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(mainCamera.transform);
#endif

            // ��ȷ��������
            RoundTransformValues(mainCamera.transform);
        }
    }

    private void RoundTransformValues(Transform t)
    {
        t.localPosition = new Vector3(
            Mathf.Round(t.localPosition.x * 1000) / 1000,
            Mathf.Round(t.localPosition.y * 1000) / 1000,
            Mathf.Round(t.localPosition.z * 1000) / 1000
        );

        Vector3 euler = t.localEulerAngles;
        t.localRotation = Quaternion.Euler(
            Mathf.Round(euler.x),
            Mathf.Round(euler.y),
            Mathf.Round(euler.z)
        );
    }

    // �༭�����ߣ��ֶ��������ù���
    [ContextMenu("�������������")]
    public void TestResetCamera()
    {
        ResetMainCamera();
        Debug.Log("��������������ر任");
    }
}