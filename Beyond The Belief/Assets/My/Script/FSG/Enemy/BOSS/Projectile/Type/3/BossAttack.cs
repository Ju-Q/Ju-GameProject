using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAttack : EnemyAttack
{
    [Header("References")]
    public Transform chargePoint;                     // 蓄能特效与发射起点
    public GameObject chargeEffectPrefab;             // 蓄能特效预制体
    public GameObject homingBulletPrefab;             // 追踪子弹预制体
    public GameObject explosionEffectPrefab;          // 爆炸特效预制体
    public Animator animator;                         // 可选：Boss Animator

    [Header("Animation / Timing")]
    public string chargeTrigger = "Charge";           // Animator Trigger 名称
    public bool useAnimatorState = true;              // 是否等待 Animator 状态结束
    public string chargeStateName = "Boss_Charge";    // Animator 状态名
    public float animationTimeout = 3f;               // 等待 Animator 进入状态的超时时间
    public float chargeTime = 2f;                     // 如果不使用动画则直接按时间
    public float chargeEffectFadeDuration = 0.8f;     // 蓄能特效渐隐时间

    [Header("Bullet")]
    public float bulletSpeed = 18f;                   // 追踪速度
    public float bulletDropSpeed = 1.2f;              // 高度下降速度
    public float bulletLifeTime = 8f;                 // 最大存在时间

    [Header("Attack Delay")]
    [Tooltip("发射后再等待多少秒才算攻击完成（包含激活动画+发射后间隔）")]
    public float postAttackDelay = 1.0f;

    [Header("Target")]
    public Transform target; // 这里直接在 Inspector 拖玩家，或在运行时赋值

    private GameObject currentChargeEffect;

    public override IEnumerator ExecuteAttack()
    {
        // 1) 播放动画
        if (animator != null && !string.IsNullOrEmpty(chargeTrigger))
        {
            animator.SetTrigger(chargeTrigger);
        }

        // 2) 生成蓄能特效
        if (chargeEffectPrefab != null && chargePoint != null)
        {
            currentChargeEffect = Instantiate(chargeEffectPrefab, chargePoint.position, chargePoint.rotation, chargePoint);
        }

        // 3) 等待动画或时间
        if (useAnimatorState && animator != null && !string.IsNullOrEmpty(chargeStateName))
        {
            yield return StartCoroutine(WaitForAnimatorStateToFinish(chargeStateName, animationTimeout));
        }
        else
        {
            yield return new WaitForSeconds(chargeTime);
        }

        // 4) 渐隐蓄能特效
        if (currentChargeEffect != null)
        {
            StartCoroutine(FadeAndDestroyEffect(currentChargeEffect, chargeEffectFadeDuration));
            currentChargeEffect = null;
        }

        // 5) 发射追踪弹
        SpawnHomingBullet();

        // 6) 发射后再等一段时间（发射间隔）
        if (postAttackDelay > 0f)
        {
            yield return new WaitForSeconds(postAttackDelay);
        }
    }

    private void SpawnHomingBullet()
    {
        if (homingBulletPrefab == null) return;

        Vector3 spawnPos = chargePoint != null ? chargePoint.position : transform.position;
        Quaternion spawnRot = chargePoint != null ? chargePoint.rotation : Quaternion.identity;

        GameObject bulletGO = Instantiate(homingBulletPrefab, spawnPos, spawnRot);
        HomingBullet hb = bulletGO.GetComponent<HomingBullet>();
        if (hb == null) hb = bulletGO.AddComponent<HomingBullet>();

        hb.Init(target, bulletSpeed, bulletDropSpeed, explosionEffectPrefab, bulletLifeTime);
    }

    private IEnumerator WaitForAnimatorStateToFinish(string stateName, float timeout)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            yield break;

        int targetHash = Animator.StringToHash(stateName);
        float timer = 0f;
        int layer = 0;

        // 等待进入目标状态
        while (timer < timeout)
        {
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            if (info.shortNameHash == targetHash || info.IsName(stateName))
            {
                // 进入目标状态，等待播放完
                while (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
                {
                    yield return null;
                }
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeAndDestroyEffect(GameObject effect, float duration)
    {
        if (effect == null)
            yield break;

        // 停止所有 ParticleSystem 的发射
        ParticleSystem[] pss = effect.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in pss)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        yield return new WaitForSeconds(duration);
        Destroy(effect);
    }
}
