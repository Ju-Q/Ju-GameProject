using UnityEngine;

public class BossCore : MonoBehaviour
{
    [Header("是否自动查找Boss")]
    public bool autoFindBoss = true;

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
            bossController.RegisterCore(this);
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

}
