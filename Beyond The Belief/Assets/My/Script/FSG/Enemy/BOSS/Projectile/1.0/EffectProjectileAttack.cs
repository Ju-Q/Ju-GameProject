using UnityEngine;
using System.Collections;

public class EffectProjectileAttack: EnemyAttack
{
    [Header("效果绑定")]
    public ParticleSystem[] ParticleEffects;   // 粒子特效
    public GameObject ProjectilePrefab;        // 子弹 prefab
    public Transform FirePoint;                // 发射点

    [Header("攻击参数")]
    public int BurstCount = 3;        // 连发次数
    public float EachFireRate = 0.3f; // 连发内部间隔
    public float FireRate = 3f;       // 攻击间隔

    [Header("目标")]
    public Transform Player; // 瞄准的玩家

    public override IEnumerator ExecuteAttack()
    {
        if (Player == null || FirePoint == null)
            yield break;

        // 三连发
        for (int i = 0; i < BurstCount; i++)
        {
            // 瞄准主角
            Vector3 lookDelta = Player.position - FirePoint.position;
            FirePoint.rotation = Quaternion.LookRotation(lookDelta);

            // 播放粒子特效
            foreach (var ps in ParticleEffects)
                ps.Emit(1);

            // 生成子弹
            if (ProjectilePrefab != null)
            {
                Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            }

            // 连发内部间隔
            yield return new WaitForSeconds(EachFireRate);
        }

        // 完成一次攻击后等待 FireRate
        yield return new WaitForSeconds(FireRate);
    }
}
