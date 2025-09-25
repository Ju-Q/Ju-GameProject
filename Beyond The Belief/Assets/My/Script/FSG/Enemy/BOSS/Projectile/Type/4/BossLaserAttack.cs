using UnityEngine;
using System.Collections;

public class BossLaserAttack : EnemyAttack
{
    [Header("Laser Settings")]
    public MagicArsenal.MagicBeamScript beamScript; // 拖入MagicBeamScript组件（挂在Boss炮台）
    public Transform target;                        // 主角或玩家角色Transform

    public override IEnumerator ExecuteAttack()
    {
        if (beamScript == null || target == null)
        {
            Debug.LogWarning("BossLaserAttack: beamScript 或 target 没有设置！");
            yield break;
        }

        // 调用MagicBeamScript的攻击逻辑
        yield return beamScript.ExecuteAttack(target);
    }
}
