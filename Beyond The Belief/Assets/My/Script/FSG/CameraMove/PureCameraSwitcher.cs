using UnityEngine;

[RequireComponent(typeof(Collider))] // 确保脚本所在对象有Collider
public class PureCameraSwitcher : MonoBehaviour
{
    [Header("摄像机设置")]
    public Camera playerCamera;  // 玩家常规摄像机
    public Camera fixedCamera;   // 固定视角摄像机

    [Header("触发器设置")]
    [Tooltip("自动获取挂载在同一对象上的Collider")]
    public Collider triggerCollider;

    private void Awake()
    {
        // 自动获取Collider组件
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider>();

        // 确保Collider是触发器
        triggerCollider.isTrigger = true;
    }

    private void Start()
    {
        // 初始化摄像机状态
        playerCamera.enabled = true;
        fixedCamera.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 使用LayerMask更可靠的检测玩家
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("玩家进入触发区域");
            SwitchToFixedView();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("玩家离开触发区域");
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

    // 调试工具
    [ContextMenu("测试切换到固定视角")]
    public void TestFixedView()
    {
        SwitchToFixedView();
    }

    [ContextMenu("测试切换到玩家视角")]
    public void TestPlayerView()
    {
        SwitchToPlayerView();
    }
}