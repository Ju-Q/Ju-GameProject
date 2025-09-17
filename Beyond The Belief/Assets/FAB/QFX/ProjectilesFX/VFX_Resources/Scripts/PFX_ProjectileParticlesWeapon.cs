#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
#endif

using UnityEngine;
using System.Collections;

public class PFX_ProjectileParticlesWeaponAttack : EnemyAttack
{
    public ParticleSystem[] ParticleSystems;
    public float FireRate = 3f;       // 每次三连发之间的间隔
    public float EachFireRate = 0.3f; // 三连发内部每颗子弹间隔
    public int BurstCount = 3;        // 连发次数
    public GameObject ProjectilePrefab;
    public Transform FirePoint; // 炮弹发射点

    public UnityEngine.Transform Player; // 主角

    public override IEnumerator ExecuteAttack()
    {
        if (Player == null)
            yield break;

        // 内部三连发
        for (int i = 0; i < BurstCount; i++)
        {
            // 瞄准主角
            var lookDelta = Player.position - transform.position;
            transform.rotation = Quaternion.LookRotation(lookDelta);
            FirePoint.LookAt(Player);

            // 发射粒子
            foreach (var ps in ParticleSystems)
                ps.Emit(1);

            // 生成碰撞体子弹
            if (ProjectilePrefab != null && FirePoint != null)
            {
                Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            }

            // 三连发内部间隔
            yield return new WaitForSeconds(EachFireRate);
        }

        // 三连发之间的间隔
        yield return new WaitForSeconds(FireRate);
    }
}
