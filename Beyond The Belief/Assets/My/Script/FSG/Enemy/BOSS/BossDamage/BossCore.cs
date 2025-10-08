using UnityEngine;

public class BossCore : MonoBehaviour
{
    [Header("是否自动查找Boss")]
    public bool autoFindBoss = true;

    [Header("核心类型")]
    public bool isFinalCore = false; // 标记是否为最终核心

    private bool isDestroyed = false;
    private BossController bossController;

    private void Start()
    {
        if (autoFindBoss)
        {
            // 先找父物体
            bossController = GetComponentInParent<BossController>();

            // 如果没找到，尝试全局找（适合独立物体）
            if (bossController == null)
                bossController = FindObjectOfType<BossController>();
        }

        // 注册自己
        if (bossController != null)
        {
            if (isFinalCore)
            {
                // 最终核心需要特殊注册
                bossController.finalCore = this;
            }
            else
            {
                // 阶段核心正常注册
                bossController.RegisterPhaseCore(this);
            }
        }
        else
        {
            Debug.LogWarning($"{name} 没有绑定到 BossController！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;

        if (other.CompareTag("SkillTrigger")) // 主角攻击器的Tag
        {
            isDestroyed = true;

            // 通知Boss
            if (bossController != null)
            {
                bossController.CoreDestroyed(this);
            }

            // TODO: 播放核心被摧毁特效
            gameObject.SetActive(false); // 或者 Destroy(gameObject)
        }
    }

    /// <summary>
    /// 重置核心状态（用于Boss醒来时恢复）
    /// </summary>
    public void ResetCore()
    {
        isDestroyed = false;
        gameObject.SetActive(true);

        // 可以在这里添加重置特效、恢复材质等逻辑
        //Debug.Log($"核心 {name} 已重置");
    }

    /// <summary>
    /// 获取核心是否已被破坏
    /// </summary>
    public bool IsDestroyed => isDestroyed;

    /// <summary>
    /// 强制设置核心状态（用于调试）
    /// </summary>
    public void SetDestroyed(bool destroyed)
    {
        isDestroyed = destroyed;
        gameObject.SetActive(!destroyed);
    }
}