using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossBombardmentAttack : EnemyAttack // 继承 EnemyAttack
{
    [Header("炮弹预制体")]
    public GameObject bombPrefab;

    [Header("提示特效")]
    public GameObject warningPrefab; // 提示特效预制体
    public float warningDuration = 1.5f; // 提示显示到攻击之间的时间

    [Header("每层楼落点")]
    public Transform[] floor1Positions;
    public Transform[] floor2Positions;
    public Transform[] floor3Positions;

    [Header("攻击参数")]
    public int bombsPerAttack = 3;             // 一次攻击生成几枚炮弹
    public float delayBetweenBombs = 0.2f;     // 同一波里每颗炮弹的间隔时间
    public float delayBetweenActivations = 2f; // 每次被激活后的冷却时间（下一次激活前等待多少秒）

    [Header("楼层高度范围")]
    public float floor1MaxHeight = 3f; // 一楼最大高度
    public float floor2MaxHeight = 6f; // 二楼最大高度

    private Transform player; // 主角位置

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    /// <summary>
    /// 由 EnemyAttackController 调用
    /// </summary>
    public override IEnumerator ExecuteAttack()
    {
        Transform[] currentFloorPositions = GetCurrentFloorPositions();
        if (currentFloorPositions == null || currentFloorPositions.Length == 0)
        {
            Debug.LogWarning("当前楼层没有预设落点！");
            yield break;
        }

        // 随机挑选落点
        List<int> usedIndexes = new List<int>();
        List<Transform> chosenPositions = new List<Transform>();

        for (int i = 0; i < bombsPerAttack; i++)
        {
            int index;
            do
            {
                index = Random.Range(0, currentFloorPositions.Length);
            } while (usedIndexes.Contains(index) && usedIndexes.Count < currentFloorPositions.Length);

            usedIndexes.Add(index);
            chosenPositions.Add(currentFloorPositions[index]);
        }

        // 先生成提示特效
        List<GameObject> warnings = new List<GameObject>();
        if (warningPrefab != null)
        {
            foreach (var pos in chosenPositions)
            {
                GameObject warn = Instantiate(warningPrefab, pos.position, pos.rotation);
                warnings.Add(warn);
            }
        }

        // 等待提示时间
        if (warningDuration > 0)
            yield return new WaitForSeconds(warningDuration);

        // 销毁提示（如果你希望提示在攻击时消失）
        foreach (var w in warnings)
        {
            if (w != null)
                Destroy(w);
        }

        // 生成炮弹
        for (int i = 0; i < chosenPositions.Count; i++)
        {
            Transform spawnPoint = chosenPositions[i];
            Instantiate(bombPrefab, spawnPoint.position, spawnPoint.rotation);

            if (delayBetweenBombs > 0 && i < chosenPositions.Count - 1)
                yield return new WaitForSeconds(delayBetweenBombs); // 每颗炮弹的间隔
        }

        // 波次之间的间隔
        if (delayBetweenActivations > 0)
            yield return new WaitForSeconds(delayBetweenActivations);
    }

    private Transform[] GetCurrentFloorPositions()
    {
        float y = player.position.y;

        if (y < floor1MaxHeight) // 一楼高度范围
            return floor1Positions;
        else if (y >= floor1MaxHeight && y < floor2MaxHeight) // 二楼高度范围
            return floor2Positions;
        else // 三楼
            return floor3Positions;
    }
}
