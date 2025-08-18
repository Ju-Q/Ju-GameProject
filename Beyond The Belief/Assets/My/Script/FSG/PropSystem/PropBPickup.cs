using UnityEngine;

// 这个脚本用于处理玩家拾取道具B的逻辑
public class PropBPickup : MonoBehaviour
{
    // 引用 SkillPointManager 脚本，用于管理技能点
    public SkillPointManager skillPointManager;
    private ItemPickupManager itemPickupManager;

    // 游戏开始时自动调用
    void Start()
    {
        // 在场景中查找 SkillPointManager 的实例并赋值给 skillPointManager
        skillPointManager = FindObjectOfType<SkillPointManager>();
        itemPickupManager = GameObject.FindGameObjectWithTag("Player").GetComponent<ItemPickupManager>();
    }

    // 当玩家进入触发区域并按下 F 键时执行
    private void OnTriggerStay(Collider other)
    {
        // 检查碰撞对象是否是玩家（Tag 为 "Player"）并且是否按下 F 键
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F) && itemPickupManager.propACount >= 1)
        {
            // 增加技能点
            skillPointManager?.AddSkillPoint();

            // 只禁用该物体，不销毁
            gameObject.SetActive(false);
        }
    }
}
