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

    // ✅ 新增：是否锁定在某一层
    private bool floorLocked = false;

    // ✅ 新增：被锁定的楼层数据
    private FireWallFloorData lockedFloor = null;

    // ✅ 可选：指定哪个楼层为一楼（或锁定目标楼层）
    [Header("楼层锁定设置")]
    [Tooltip("一楼的楼层名称（检测用）")]
    public string floorNameToLock = "一楼";

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

        // ✅ 如果已经锁定楼层，直接使用锁定楼层
        FireWallFloorData currentFloor = floorLocked ? lockedFloor : GetFloorByHeight(playerY);

        // ✅ 如果还没锁定楼层，并且检测到进入指定楼层（例如一楼）
        if (!floorLocked && currentFloor != null && currentFloor.floorName == floorNameToLock)
        {
            lockedFloor = currentFloor;
            floorLocked = true;
            Debug.Log($"🔥 FireWallAttack 已锁定楼层：{lockedFloor.floorName}");
        }

        // ✅ 如果仍然没找到楼层（且没锁定），直接退出
        if (currentFloor == null)
        {
            Debug.LogWarning("FireWallAttack: 未找到对应楼层！");
            yield break;
        }

        // ✅ 从锁定或当前楼层中随机选方向
        if (currentFloor.directions == null || currentFloor.directions.Length == 0)
        {
            Debug.LogWarning("FireWallAttack: 当前楼层没有配置方向！");
            yield break;
        }

        int chosenIndex = Random.Range(0, currentFloor.directions.Length);
        if (currentFloor.directions.Length > 1)
        {
            int attempts = 0;
            while (chosenIndex == lastDirectionIndex && attempts < 10)
            {
                chosenIndex = Random.Range(0, currentFloor.directions.Length);
                attempts++;
            }
        }

        lastDirectionIndex = chosenIndex;
        FireWallDirectionData chosenDirData = currentFloor.directions[chosenIndex];

        // 🔥 提示特效开启
        if (chosenDirData.warningObject != null)
            chosenDirData.warningObject.SetActive(true);

        yield return new WaitForSeconds(warningTime);

        // 🔥 生成火焰墙
        if (fireWallPrefab != null && chosenDirData.fireWallSpawnPoint != null)
        {
            GameObject fireWall = GameObject.Instantiate(
                fireWallPrefab,
                chosenDirData.fireWallSpawnPoint.position,
                Quaternion.identity
            );

            FireWallMover mover = fireWall.GetComponent<FireWallMover>();
            if (mover != null)
                mover.Init(chosenDirData.direction);

            GameObject.Destroy(fireWall, fireWallDuration);
        }

        // 🔥 提示特效关闭
        if (chosenDirData.warningObject != null)
            chosenDirData.warningObject.SetActive(false);

        yield return new WaitForSeconds(intervalBetweenActivations);
    }

    // ✅ 工具方法：通过高度找到楼层
    private FireWallFloorData GetFloorByHeight(float y)
    {
        foreach (var floor in floors)
        {
            if (y >= floor.minHeight && y < floor.maxHeight)
                return floor;
        }
        return null;
    }
}
