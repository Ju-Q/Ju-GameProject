using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialActivator : EnemyAttack
{
    [Header("需要依次开启的物体列表")]
    public List<GameObject> objectsToActivate = new List<GameObject>();

    private int currentIndex = 0; // 当前下一个要激活的物体索引

    public override IEnumerator ExecuteAttack()
    {
        // 找到下一个未激活物体
        while (currentIndex < objectsToActivate.Count)
        {
            GameObject obj = objectsToActivate[currentIndex];
            currentIndex++; // 索引推进，不管是否激活过

            if (obj != null && !obj.activeSelf)
            {
                obj.SetActive(true);
                Debug.Log($"[SequentialActivator] 激活 {obj.name}");
                break; // 每次 ExecuteAttack 只激活一个
            }
        }

        yield return null;
    }

    // 保持物体状态，不自动关闭，只重置索引用于下一次阶段调用
    public override void ResetAttack()
    {
        currentIndex = 0;
        //Debug.Log("[SequentialActivator] 索引已重置，但物体保持开启状态");
    }
}
