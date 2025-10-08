using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyAttackPhase
{
    public string PhaseName;
    public List<EnemyAttack> AttacksInThisPhase;
    public float PhaseDuration = 5f;
    public float IntervalAfterPhase = 2f;
    public bool IsCheckpoint = false;
}

[System.Serializable]
public class PhaseLoopCondition
{
    [Header("循环范围 (包含开始和结束)")]
    public int LoopStartIndex = 0;    // 循环起点阶段
    public int LoopEndIndex = 0;      // 循环终点阶段

    [Header("退出循环所需破坏核心数")]
    public int RequiredDestroyedCores = 1;
}

public class EnemyAttackController : MonoBehaviour
{
    [Header("Boss阶段列表")]
    public List<EnemyAttackPhase> AttackPhases;

    [Header("阶段回溯设置")]
    public List<int> CheckpointPhaseIndices = new List<int>();

    [Header("Checkpoint位置")]
    public List<Transform> CheckpointRespawnPoints = new List<Transform>();

    [Header("循环设置")]
    public List<PhaseLoopCondition> LoopConditions = new List<PhaseLoopCondition>();

    [Header("控制Boss是否可以开始攻击")]
    public bool CanStartAttack = false;

    [Header("调试")]
    public bool enableDebugLogs = true;

    public int CurrentPhaseIndex { get; private set; } = -1;
    public EnemyAttackPhase CurrentPhase =>
        (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < AttackPhases.Count)
        ? AttackPhases[CurrentPhaseIndex] : null;
    public bool IsPaused { get; private set; } = false;

    private Coroutine phasesCoroutine;
    private List<Coroutine> runningAttacks = new List<Coroutine>();
    private bool isRewinding = false;
    private bool previousCanStartAttack = false;
    private bool isDead = false; //Boss是否死亡

    private BossController bossController; // 需要用来查询核心破坏数

    private void Start()
    {
        previousCanStartAttack = CanStartAttack;
        bossController = GetComponent<BossController>();
    }

    private void Update()
    {
        // false -> true
        if (!previousCanStartAttack && CanStartAttack)
        {
            if (IsPaused)
            {
                ResumeBoss(CurrentPhaseIndex >= 0 ? CurrentPhaseIndex : 0);
            }
            else
            {
                StartPhasesFromIndex(CurrentPhaseIndex >= 0 ? CurrentPhaseIndex : 0);
            }
        }
        // true -> false
        else if (previousCanStartAttack && !CanStartAttack)
        {
            PauseBoss();
        }

        previousCanStartAttack = CanStartAttack;
    }

    private IEnumerator PerformPhases(int startIndex)
    {
        for (int i = startIndex; i < AttackPhases.Count; i++)
        {
            CurrentPhaseIndex = i;
            EnemyAttackPhase phase = AttackPhases[i];

            if (enableDebugLogs)
                Debug.Log($"[Boss] Enter Phase {i}: {phase.PhaseName}");

            // 启动该阶段攻击
            runningAttacks.Clear();
            foreach (var attack in phase.AttacksInThisPhase)
            {
                Coroutine c = StartCoroutine(RepeatAttackInPhase(attack, phase.PhaseDuration));
                runningAttacks.Add(c);
            }

            yield return new WaitForSeconds(phase.PhaseDuration);

            StopAllAttackCoroutines();

            if (phase.IntervalAfterPhase > 0f)
            {
                CurrentPhaseIndex = -1;
                yield return new WaitForSeconds(phase.IntervalAfterPhase);
            }

            if (!CanStartAttack)
                yield break;

            // ✅ 循环判定
            // ✅ 循环判定（只在循环区最后一个阶段才检查）
            PhaseLoopCondition loop = GetLoopForPhase(i);
            if (loop != null && i == loop.LoopEndIndex)
            {
                int destroyedCores = bossController != null ? bossController.DestroyedCoresCount : 0;

                if (destroyedCores < loop.RequiredDestroyedCores)
                {
                    // 没有达到条件，回到循环起点
                    if (enableDebugLogs)
                        Debug.Log($"[Boss] Not enough cores destroyed ({destroyedCores}/{loop.RequiredDestroyedCores}), looping back to phase {loop.LoopStartIndex}");

                    i = loop.LoopStartIndex - 1; // 下一轮会变成 LoopStartIndex
                    continue;
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.Log($"[Boss] Required cores destroyed ({destroyedCores}), breaking loop at phase {i}");
                }
            }

        }

        CurrentPhaseIndex = -1;

        if (enableDebugLogs)
            Debug.Log("[Boss] All phases completed.");
    }

    private PhaseLoopCondition GetLoopForPhase(int phaseIndex)
    {
        foreach (var loop in LoopConditions)
        {
            if (phaseIndex >= loop.LoopStartIndex && phaseIndex <= loop.LoopEndIndex)
                return loop;
        }
        return null;
    }

    private IEnumerator RepeatAttackInPhase(EnemyAttack attack, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            yield return attack.ExecuteAttack();
            yield return null;
            timer += Time.deltaTime;

            if (!CanStartAttack)
                yield break;
        }
    }

    private void StopAllAttackCoroutines()
    {
        foreach (var c in runningAttacks)
        {
            if (c != null)
                StopCoroutine(c);
        }
        runningAttacks.Clear();

        // ❌ 不再在这里 ResetAttack()
        // 子弹不会被清空
    }

    // 新增：真正需要清理攻击的时候才调用
    private void ResetAllAttacks()
    {
        foreach (var phase in AttackPhases)
        {
            foreach (var attack in phase.AttacksInThisPhase)
            {
                if (attack != null)
                {
                    attack.ResetAttack();
                    if (enableDebugLogs)
                        Debug.Log($"[Boss] ResetAttack called for {attack.name}");
                }
            }
        }
    }


    public void StartPhasesFromIndex(int index)
    {
        if (enableDebugLogs)
            Debug.Log($"[Boss] StartPhasesFromIndex called with index {index}");

        if (phasesCoroutine != null)
            StopCoroutine(phasesCoroutine);

        StopAllAttackCoroutines();

        phasesCoroutine = StartCoroutine(PerformPhases(index));
    }

    public void RewindToCheckpoint()
    {
        if (isRewinding) return;
        isRewinding = true;

        // ⚠️ 新增：先暂停
        PauseBoss();

        int rewindIndex = GetCheckpointIndexForCurrentPhase();
        if (enableDebugLogs)
            Debug.Log($"[Boss] Rewinding to checkpoint phase {rewindIndex}");

        // 只重置当前阶段索引，不直接开始攻击
        CurrentPhaseIndex = rewindIndex;

        isRewinding = false;
    }

    private int GetCheckpointIndexForCurrentPhase()
    {
        if (CheckpointPhaseIndices == null || CheckpointPhaseIndices.Count == 0)
            return 0;

        int result = CheckpointPhaseIndices[0];
        for (int i = 0; i < CheckpointPhaseIndices.Count; i++)
        {
            if (CheckpointPhaseIndices[i] <= CurrentPhaseIndex)
                result = CheckpointPhaseIndices[i];
        }
        return result;
    }

    public void PauseBoss()
    {
        if (IsPaused) return;
        IsPaused = true;
        StopAllAttackCoroutines();
        if (phasesCoroutine != null)
            StopCoroutine(phasesCoroutine);
        if (enableDebugLogs)
            Debug.Log("[Boss] Paused");
    }

    public void ResumeBoss(int startPhaseIndex = -1, float delay = 0f)
    {
        if (!CanStartAttack)
        {
            if (enableDebugLogs) Debug.Log("[Boss] ResumeBoss skipped because CanStartAttack=false");
            return;
        }

        if (!IsPaused) return;
        IsPaused = false;

        StartCoroutine(ResumeBossWithDelay(startPhaseIndex, delay));

        if (enableDebugLogs)
            Debug.Log("[Boss] Resuming with delay: " + delay + "s");
    }

    private IEnumerator ResumeBossWithDelay(int startPhaseIndex, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (startPhaseIndex >= 0)
            StartPhasesFromIndex(startPhaseIndex);
        else
            StartPhasesFromIndex(CurrentPhaseIndex >= 0 ? CurrentPhaseIndex : 0);
    }

    public Vector3 GetRespawnPositionForCurrentPhase()
    {
        int checkpointIndex = GetCheckpointIndexForCurrentPhase();
        int listIndex = CheckpointPhaseIndices.IndexOf(checkpointIndex);

        if (listIndex >= 0 && listIndex < CheckpointRespawnPoints.Count)
            return CheckpointRespawnPoints[listIndex].position;

        return Vector3.zero;
    }

    public void OnBossDeath()
    {
        if (isDead) return;
        isDead = true;

        PauseBoss(); // 停止一切攻击与阶段
        if (enableDebugLogs)
            Debug.Log("[Boss] AttackController stopped because Boss is dead.");
    }

    // 新增：Boss晕厥时调用
    public void OnBossStun()
    {
        if (enableDebugLogs)
            Debug.Log("[Boss] Boss stunned, pausing attack controller");

        // 暂停Boss攻击
        PauseBoss();

        // 可以选择性地重置所有攻击状态
        ResetAllAttacks();
    }

    // 新增：Boss醒来时调用
    public void OnBossWakeUp()
    {
        if (enableDebugLogs)
            Debug.Log("[Boss] Boss woke up, attack controller remains paused until CanStartAttack=true");

        // Boss醒来后，攻击控制器保持暂停状态
        // 实际的恢复由CanStartAttack=true触发
        // 这里可以添加一些醒来时的特殊逻辑
    }

    // 确保OnBossDeath方法也存在（你已经有这个了，但为了完整性再确认一下）
    /*public void OnBossDeath()
    {
        if (isDead) return;
        isDead = true;

        PauseBoss(); // 停止一切攻击与阶段
        ResetAllAttacks(); // 清理所有攻击状态

        if (enableDebugLogs)
            Debug.Log("[Boss] AttackController stopped because Boss is dead.");
    }*/


}
