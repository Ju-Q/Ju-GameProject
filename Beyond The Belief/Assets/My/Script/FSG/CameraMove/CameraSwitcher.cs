using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("摄像机设置")]
    public Camera mainCamera;      // 主摄像机
    public Camera tradeCamera;    // 交易区摄像机
    public Transform cameraPosition; // 摄像机复位位置（本地坐标归零的父对象）

    [Header("触发区域")]
    public Collider tradeZone;    // 交易区碰撞体

    private bool isInTradeZone = false;

    private void Start()
    {
        // 初始化摄像机状态
        mainCamera.enabled = true;
        tradeCamera.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInTradeZone)
        {
            // 切换到交易区摄像机
            mainCamera.enabled = false;
            tradeCamera.enabled = true;
            isInTradeZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInTradeZone)
        {
            // 切换回主摄像机
            tradeCamera.enabled = false;
            mainCamera.enabled = true;
            isInTradeZone = false;

            // 重置主摄像机本地变换
      //      ResetMainCamera();
        }
    }

    private void ResetMainCamera()
    {
        if (cameraPosition != null)
        {
            // 确保先设置父对象
            mainCamera.transform.SetParent(cameraPosition);

            // 硬性重置变换
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
            mainCamera.transform.localScale = Vector3.one;
            // 通过底层TransformAccess接口（需要UnityEngine.Experimental模块）
            var transformAccess = mainCamera.transform;
            transformAccess.localPosition = Vector3.zero;
            transformAccess.localRotation = Quaternion.identity;
            transformAccess.localScale = Vector3.one;

            // 强制重新计算世界矩阵
            mainCamera.SendMessage("OnTransformChanged", SendMessageOptions.DontRequireReceiver);

            // 强制刷新
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(mainCamera.transform);
#endif

            // 精确四舍五入
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

    // 编辑器工具：手动测试重置功能
    [ContextMenu("测试重置摄像机")]
    public void TestResetCamera()
    {
        ResetMainCamera();
        Debug.Log("已重置摄像机本地变换");
    }
}