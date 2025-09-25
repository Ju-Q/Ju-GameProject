using UnityEngine;
using System.Collections;

[System.Serializable]
public class FireWallDirectionData
{
    [Tooltip("火焰墙推进方向，例如 (0,0,1)=前, (0,0,-1)=后, (1,0,0)=右, (-1,0,0)=左")]
    public Vector3 direction;

    [Header("提示特效（场景已有对象，默认关闭）")]
    public GameObject warningObject; // 提示特效对象（场景中）

    [Header("火焰墙生成位置（空物体即可）")]
    public Transform fireWallSpawnPoint; // 这个方向火焰墙的生成位置（空物体）
}

[System.Serializable]
public class FireWallFloorData
{
    [Header("楼层信息")]
    public string floorName;   // 楼层名称
    public float minHeight;    // 玩家 y >= minHeight
    public float maxHeight;    // 玩家 y < maxHeight

    [Header("方向设置（每个方向一个提示 + 一个生成点）")]
    public FireWallDirectionData[] directions;
}

[CreateAssetMenu(menuName = "EnemyAttacks/FireWallAttack")]
public class FireWallAttack : EnemyAttack
{
    [Header("楼层设置")]
    public FireWallFloorData[] floors;

    [Header("火焰墙统一特效（Prefab）")]
    public GameObject fireWallPrefab;

    [Header("攻击控制")]
    [Tooltip("每次激活之间的间隔（秒）")]
    public float intervalBetweenActivations = 1f;

    [Tooltip("提示特效提前出现的时间（秒）")]
    public float warningTime = 2f;

    [Tooltip("火焰墙持续时间（秒）")]
    public float fireWallDuration = 3f;

    // 记录上一次选择的方向索引（避免重复）
    private int lastDirectionIndex = -1;

    public override IEnumerator ExecuteAttack()
    {
        // 找到玩家
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("FireWallAttack: Player 未找到！");
            yield break;
        }

        float playerY = player.transform.position.y;

        // 找到玩家所在楼层
        FireWallFloorData currentFloor = null;
        foreach (var floor in floors)
        {
            if (playerY >= floor.minHeight && playerY < floor.maxHeight)
            {
                currentFloor = floor;
                break;
            }
        }

        if (currentFloor == null)
        {
            Debug.LogWarning("FireWallAttack: 玩家未在任何楼层范围内！");
            yield break;
        }

        // 随机选取一个方向（避免和上一次相同）
        if (currentFloor.directions == null || currentFloor.directions.Length == 0)
        {
            Debug.LogWarning("FireWallAttack: 当前楼层没有配置方向！");
            yield break;
        }

        int chosenIndex = Random.Range(0, currentFloor.directions.Length);
        if (currentFloor.directions.Length > 1)
        {
            // 如果只有一个方向就不用管
            int attempts = 0;
            while (chosenIndex == lastDirectionIndex && attempts < 10)
            {
                chosenIndex = Random.Range(0, currentFloor.directions.Length);
                attempts++;
            }
        }

        lastDirectionIndex = chosenIndex; // 记录本次方向索引
        FireWallDirectionData chosenDirData = currentFloor.directions[chosenIndex];

        // 提示特效开启
        if (chosenDirData.warningObject != null)
        {
            chosenDirData.warningObject.SetActive(true);
        }

        // 等待提示时间
        yield return new WaitForSeconds(warningTime);

        // 生成火焰墙
        if (fireWallPrefab != null && chosenDirData.fireWallSpawnPoint != null)
        {
            GameObject fireWall = GameObject.Instantiate(
                fireWallPrefab,
                chosenDirData.fireWallSpawnPoint.position,
                Quaternion.identity
            );

            // 如果火焰墙有 mover，初始化方向
            FireWallMover mover = fireWall.GetComponent<FireWallMover>();
            if (mover != null)
            {
                mover.Init(chosenDirData.direction);
            }

            // 火焰墙持续时间后自动销毁
            GameObject.Destroy(fireWall, fireWallDuration);
        }

        // 提示特效关闭
        if (chosenDirData.warningObject != null)
        {
            chosenDirData.warningObject.SetActive(false);
        }

        // 等待间隔再执行下一次攻击
        yield return new WaitForSeconds(intervalBetweenActivations);
    }
}
