using UnityEngine;
using System.Collections;

public class BossRotatingLaserAttack : EnemyAttack
{
    [Header("Laser Settings")]
    public MagicArsenal.MagicRotatingBeam rotatingBeamScript; // 拖入MagicRotatingBeam组件（挂在Boss本体或激光管理器）

    public float activeDuration = 5f; // 激光持续时间

    public override IEnumerator ExecuteAttack()
    {
        if (rotatingBeamScript == null)
        {
            Debug.LogWarning("BossRotatingLaserAttack: rotatingBeamScript 没有设置！");
            yield break;
        }

        // 激活旋转光束
        rotatingBeamScript.ActivateBeams();

        // 持续一段时间
        yield return new WaitForSeconds(activeDuration);

        // 停止旋转光束
        rotatingBeamScript.DeactivateBeams();
    }
}
