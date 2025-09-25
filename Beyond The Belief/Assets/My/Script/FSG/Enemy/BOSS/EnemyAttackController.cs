using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyAttackPhase
{
    public string PhaseName;                        // 阶段名字（可选）
    public List<EnemyAttack> AttacksInThisPhase;    // 该阶段的所有攻击
    public float PhaseDuration = 5f;               // 阶段持续时间
    public float IntervalAfterPhase = 2f;          // 阶段结束后到下一阶段的间隔时间
}

public class EnemyAttackController : MonoBehaviour
{
    public List<EnemyAttackPhase> AttackPhases;     // 所有阶段列表
    public int CurrentPhaseIndex { get; private set; } = -1;  // 当前阶段索引（-1表示未开始）
    public EnemyAttackPhase CurrentPhase =>
        (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < AttackPhases.Count)
        ? AttackPhases[CurrentPhaseIndex] : null;   // 方便外部获取当前阶段对象

    private void Start()
    {
        StartCoroutine(PerformPhases());
    }

    private IEnumerator PerformPhases()
    {
        for (int i = 0; i < AttackPhases.Count; i++)
        {
            CurrentPhaseIndex = i; // 设置当前阶段索引
            EnemyAttackPhase phase = AttackPhases[i];

            // 启动该阶段的攻击协程
            List<Coroutine> runningAttacks = new List<Coroutine>();
            foreach (var attack in phase.AttacksInThisPhase)
            {
                Coroutine c = StartCoroutine(RepeatAttackInPhase(attack, phase.PhaseDuration));
                runningAttacks.Add(c);
            }

            // 等待阶段时间
            yield return new WaitForSeconds(phase.PhaseDuration);

            // 停止所有攻击协程（结束当前阶段）
            foreach (var c in runningAttacks)
            {
                if (c != null) StopCoroutine(c);
            }

            // 阶段间隔
            if (phase.IntervalAfterPhase > 0f)
            {
                CurrentPhaseIndex = -1; // 间隔期不算在任何阶段
                yield return new WaitForSeconds(phase.IntervalAfterPhase);
            }
        }

        CurrentPhaseIndex = -1; // 所有阶段完成
    }

    private IEnumerator RepeatAttackInPhase(EnemyAttack attack, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            yield return attack.ExecuteAttack(); // 内部已处理冷却
            yield return null;
            timer += Time.deltaTime;
        }
    }
}
