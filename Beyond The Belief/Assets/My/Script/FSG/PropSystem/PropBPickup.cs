using UnityEngine;

// 这个脚本用于处理玩家拾取道具B的逻辑
public class PropBPickup : MonoBehaviour
{
    // 引用 SkillPointManager 脚本，用于管理技能点
    public SkillPointManager skillPointManager;

    // 游戏开始时自动调用
    void Start()
    {
        // 在场景中查找 SkillPointManager 的实例并赋值给 skillPointManager
        skillPointManager = FindObjectOfType<SkillPointManager>();

        // 如果第一次查找失败，再尝试查找一次（这里有点冗余，可以优化）
        if (skillPointManager == null)
        {
            skillPointManager = FindObjectOfType<SkillPointManager>();
        }
    }

    // 当玩家进入触发区域并按下 F 键时执行
    private void OnTriggerStay(Collider other)
    {
        // 检查碰撞对象是否是玩家（Tag 为 "Player"）并且是否按下 F 键
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            // 如果 skillPointManager 不为空，调用 AddSkillPoint() 方法增加技能点
            skillPointManager?.AddSkillPoint();

            // 注释掉的代码：拾取后销毁道具（可以根据需求取消注释）
            // Destroy(gameObject);
        }
    }
}