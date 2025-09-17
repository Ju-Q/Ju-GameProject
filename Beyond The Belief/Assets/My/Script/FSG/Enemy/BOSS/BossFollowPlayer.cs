using System.Collections.Generic;
using UnityEngine;

public class BossFollowPlayer : MonoBehaviour
{
    public Transform player;       // 主角
    public Transform center;       // 空心区域中心
    public float radius = 5f;      // 空心区域半径
    public float moveSpeed = 5f;   // 移动速度

    [Header("可变楼层设置")]
    public List<float> floorHeightThresholds; // 楼层分界Y坐标（n-1个阈值）
    public List<float> bossHeightOffsets;     // 每层对应Boss高度偏移（长度 = 楼层数量）

    void Update()
    {
        // 1️⃣ 水平移动
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dirFromCenter = targetPos - center.position;
        if (dirFromCenter.magnitude > radius)
            dirFromCenter = dirFromCenter.normalized * radius;
        Vector3 bossTarget = center.position + dirFromCenter;

        // 2️⃣ 根据玩家楼层调整Y坐标
        float targetY = GetBossHeight(player.position.y);
        bossTarget.y = targetY;

        // 3️⃣ 移动Boss
        transform.position = Vector3.MoveTowards(transform.position, bossTarget, moveSpeed * Time.deltaTime);

        // 4️⃣ 始终朝向玩家
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);
    }

    float GetBossHeight(float playerY)
    {
        // 默认取最后一层偏移
        float height = bossHeightOffsets[bossHeightOffsets.Count - 1];

        for (int i = 0; i < floorHeightThresholds.Count; i++)
        {
            if (playerY < floorHeightThresholds[i])
            {
                height = bossHeightOffsets[i];
                break;
            }
        }

        return center.position.y + height;
    }
}
