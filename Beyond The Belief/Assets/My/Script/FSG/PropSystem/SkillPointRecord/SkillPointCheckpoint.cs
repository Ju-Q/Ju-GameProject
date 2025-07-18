using UnityEngine;

public class SkillPointCheckpoint : MonoBehaviour
{
    // 用于标识该触发区域是否已被触发（可选）
    private bool hasTriggered = false;

    // 玩家进入触发区域时调用
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 记录当前技能点
            int currentPoints = SkillPointManager.Instance.currentSkillPoints;

            // 将值保存到全局记录器
            SkillPointRecord.Instance.RememberSkillPoints(currentPoints);

            Debug.Log($"玩家进入触发区域，记录当前技能点数为：{currentPoints}");

            // 可选：防止重复触发
            hasTriggered = true;
        }
    }
}
