using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class CheckpointCoreState
{
    public int checkpointPhaseIndex;
    public List<bool> coreActiveStates;
    public int destroyedCoresCount;
    public bool phaseTriggered;
    public List<int> destroyedCoreIndices;
}

public class BossController : MonoBehaviour
{
    [Header("Animator (可拖拽，空则自动查找子物体)")]
    public Animator animator;
    [Tooltip("每次核心被破坏触发的Trigger名")] public string hitTrigger = "Hit";
    [Tooltip("全部核心被破坏触发的Trigger名")] public string dieTrigger = "Die";
    [Tooltip("晕厥状态Trigger名")] public string stunTrigger = "Stun";
    [Tooltip("醒来状态Trigger名")] public string wakeUpTrigger = "WakeUp";

    [Header("EnemyAttackController (可拖拽，空则自动查找)")]
    public EnemyAttackController attackController;

    [Header("Boss 核心设置")]
    public List<BossCore> phaseCores = new List<BossCore>();
    public BossCore finalCore;

    [Header("阶段设置")]
    [Tooltip("触发场景变化的破坏核心数量")] public int phaseTriggerCount = 3;
    [Tooltip("晕厥持续时间（秒）")] public float stunDuration = 10f;
    [Tooltip("晕厥醒来后要重新激活的特定核心（可拖拽指定）")]
    public List<BossCore> coresToReactivateAfterStun = new List<BossCore>();

    [Header("BossFollowPlayer引用")]
    [Tooltip("手动拖拽BossFollowPlayer所在的物体")]
    public BossFollowPlayer bossFollowerManual;

    [Header("死亡设置")]
    [Tooltip("在触发死亡后延迟多少秒执行最终死亡逻辑（可用于等动画播完）")]
    public float deathDisableDelay = 0f;
    [Tooltip("在 Inspector 可以绑定死亡时要执行的动作（掉落、禁用AI等）")]
    public UnityEvent onBossDie;

    [Header("阶段事件")]
    [Tooltip("当达到指定破坏数量时触发场景变化")] public UnityEvent onPhaseTrigger;
    [Tooltip("当晕厥结束时触发（恢复场景）")] public UnityEvent onPhaseReset;
    [Tooltip("当最终核心被破坏时触发")] public UnityEvent onFinalCoreDestroyed;

    [Header("检查点核心状态记录")]
    [Tooltip("调试：显示已记录的检查点核心状态")]
    public List<CheckpointCoreState> recordedCheckpointStates = new List<CheckpointCoreState>();
    [Header("事件触发器")]
    [Tooltip("Boss晕厥时触发的事件（类似 OnPhaseTrigger）")]
    public UnityEngine.Events.UnityEvent onStunTrigger;

    [Tooltip("Boss从晕厥恢复时触发的事件")]
    public UnityEngine.Events.UnityEvent onWakeUpTrigger;

    [Header("技能资源生成设置")]
    [Tooltip("技能资源预制体，可拖拽多个")]
    public List<GameObject> skillPrefabs = new List<GameObject>();
    [Tooltip("技能资源生成位置")]
    public List<Transform> spawnPoints = new List<Transform>();
    [Tooltip("每次晕厥随机生成的技能数量")]
    public int spawnSkillCountPerStun = 3;

    // 状态变量
    private int destroyedCores = 0;
    private bool isDead = false;
    private bool isStunned = false;
    private bool phaseTriggered = false;
    private bool stunLocked = false; // 锁定晕厥状态
    private Coroutine stunCoroutine;
    private BossFollowPlayer bossFollower;
    private List<BossCore> destroyedPhaseCores = new List<BossCore>();

    // 公共属性
    public int DestroyedCoresCount => destroyedCores;
    public bool IsStunned => isStunned;
    public bool PhaseTriggered => phaseTriggered;
    public bool IsDead => isDead;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (attackController == null) attackController = GetComponent<EnemyAttackController>();

        bossFollower = bossFollowerManual != null ? bossFollowerManual : GetComponent<BossFollowPlayer>() ?? GetComponentInChildren<BossFollowPlayer>();
        if (bossFollower != null)
            Debug.Log($"✅ 找到 BossFollowPlayer: {bossFollower.gameObject.name}");
        else
            Debug.LogError("❌ 未找到 BossFollowPlayer，请手动设置 bossFollowerManual");

        if (phaseCores == null || phaseCores.Count == 0)
        {
            var found = GetComponentsInChildren<BossCore>(true);
            foreach (var c in found)
            {
                if (c != null && c != finalCore && !c.isFinalCore && !phaseCores.Contains(c))
                    phaseCores.Add(c);
            }
        }

        ValidateReactivateCores();

        if (finalCore != null) finalCore.gameObject.SetActive(false);

        destroyedCores = 0;
        isDead = false;
        isStunned = false;
        phaseTriggered = false;
        destroyedPhaseCores.Clear();
    }

    private void Start()
    {
        RecordCheckpointState(0);
    }

    // ===========================
    // 检查点记录与恢复
    // ===========================

    public void RecordCheckpointState(int checkpointPhaseIndex)
    {
        var existingState = recordedCheckpointStates.Find(s => s.checkpointPhaseIndex == checkpointPhaseIndex);
        if (existingState != null) UpdateCheckpointState(existingState);
        else recordedCheckpointStates.Add(CreateCheckpointState(checkpointPhaseIndex));
    }

    private CheckpointCoreState CreateCheckpointState(int checkpointPhaseIndex)
    {
        var state = new CheckpointCoreState
        {
            checkpointPhaseIndex = checkpointPhaseIndex,
            coreActiveStates = new List<bool>(),
            destroyedCoresCount = destroyedCores,
            phaseTriggered = phaseTriggered,
            destroyedCoreIndices = new List<int>()
        };

        for (int i = 0; i < phaseCores.Count; i++)
        {
            var core = phaseCores[i];
            bool alive = core != null && !destroyedPhaseCores.Contains(core) && core.gameObject.activeInHierarchy;
            state.coreActiveStates.Add(alive);
            if (!alive) state.destroyedCoreIndices.Add(i);
        }
        return state;
    }

    private void UpdateCheckpointState(CheckpointCoreState state)
    {
        state.destroyedCoresCount = destroyedCores;
        state.phaseTriggered = phaseTriggered;
        state.coreActiveStates.Clear();
        state.destroyedCoreIndices.Clear();

        for (int i = 0; i < phaseCores.Count; i++)
        {
            var core = phaseCores[i];
            bool alive = core != null && !destroyedPhaseCores.Contains(core) && core.gameObject.activeInHierarchy;
            state.coreActiveStates.Add(alive);
            if (!alive) state.destroyedCoreIndices.Add(i);
        }
    }

    public void RestoreToCheckpoint(int checkpointPhaseIndex)
    {
        var state = recordedCheckpointStates.Find(s => s.checkpointPhaseIndex == checkpointPhaseIndex);
        if (state == null) { Debug.LogError($"❌ 找不到检查点 {checkpointPhaseIndex}"); return; }

        destroyedCores = state.destroyedCoresCount;
        phaseTriggered = state.phaseTriggered;
        destroyedPhaseCores.Clear();

        for (int i = 0; i < phaseCores.Count && i < state.coreActiveStates.Count; i++)
        {
            var core = phaseCores[i];
            if (core != null)
            {
                if (state.coreActiveStates[i])
                {
                    core.ResetCore();
                    core.gameObject.SetActive(true);
                }
                else
                {
                    core.gameObject.SetActive(false);
                    if (state.destroyedCoreIndices.Contains(i)) destroyedPhaseCores.Add(core);
                }
            }
        }

        if (isStunned)
        {
            if (stunCoroutine != null) { StopCoroutine(stunCoroutine); stunCoroutine = null; }
            isStunned = false;
            stunLocked = false;
            bossFollower?.OnBossStunEnd();
            if (finalCore != null) finalCore.gameObject.SetActive(false);
        }

        isDead = false;
        enabled = true;

        if (animator != null) { animator.Rebind(); animator.Update(0f); }

        destroyedCores = destroyedPhaseCores.Count;

        if (!phaseTriggered && destroyedCores >= phaseTriggerCount) TriggerPhaseChange();
    }

    public void OnCheckpointReached(int checkpointPhaseIndex) => RecordCheckpointState(checkpointPhaseIndex);

    // ===========================
    // 核心破坏与阶段逻辑
    // ===========================

    public void CoreDestroyed(BossCore core)
    {
        if (isDead) return;

        if (core == finalCore && finalCore != null) { DestroyFinalCore(); return; }
        if (!phaseCores.Contains(core)) { Debug.Log("❌ 核心不在phaseCores列表中"); return; }

        destroyedCores++;
        if (!destroyedPhaseCores.Contains(core)) destroyedPhaseCores.Add(core);

        if (animator != null) animator.SetTrigger(hitTrigger);

        if (!phaseTriggered && destroyedCores >= phaseTriggerCount) TriggerPhaseChange();
        if (phaseTriggered && destroyedCores >= phaseCores.Count) EnterStunState();

        if (attackController != null && attackController.CurrentPhase != null)
        {
            int currentPhaseIndex = attackController.CurrentPhaseIndex;
            if (attackController.CheckpointPhaseIndices.Contains(currentPhaseIndex))
                RecordCheckpointState(currentPhaseIndex);
        }
    }

    private void TriggerPhaseChange()
    {
        phaseTriggered = true;
        onPhaseTrigger?.Invoke();
    }

    private void EnterStunState()
    {
        if (isStunned || stunLocked) return;
        isStunned = true;
        stunLocked = true;

        if (animator != null) animator.SetTrigger(stunTrigger);
        bossFollower?.OnBossStunStart();

        if (finalCore != null) { finalCore.gameObject.SetActive(true); finalCore.ResetCore(); }
        attackController?.OnBossStun();


        // 调用晕厥事件（可配置，例如生成物体、播放特效）
        onStunTrigger?.Invoke();

        // 如果你想在晕厥时生成技能资源
        SpawnSkillsAfterStun();


        stunCoroutine = StartCoroutine(StunCountdown());


    }

    private IEnumerator StunCountdown()
    {
        yield return new WaitForSeconds(stunDuration);
        WakeUpBoss();
    }

    private void WakeUpBoss()
    {
        if (!isStunned) return;

        isStunned = false;
        stunLocked = false;

        if (stunCoroutine != null) { StopCoroutine(stunCoroutine); stunCoroutine = null; }

        bossFollower?.OnBossStunEnd();

        if (finalCore != null) finalCore.gameObject.SetActive(false);

        ReactivateSpecificCoresAfterStun();

        if (animator != null) animator.SetTrigger(wakeUpTrigger);

        if (attackController != null)
        {
            attackController.OnBossWakeUp();
            if (attackController.IsPaused && attackController.CanStartAttack)
                attackController.ResumeBoss(attackController.CurrentPhaseIndex >= 0 ? attackController.CurrentPhaseIndex : 0);
            // ✅ 新增
            onWakeUpTrigger?.Invoke();

        }

        // 晕厥后生成技能资源
        SpawnSkillsAfterStun();

        onPhaseReset?.Invoke();
    }

    private void ReactivateSpecificCoresAfterStun()
    {
        int count = 0;
        foreach (var core in coresToReactivateAfterStun)
        {
            if (core != null && destroyedPhaseCores.Contains(core))
            {
                core.ResetCore();
                core.gameObject.SetActive(true);
                destroyedPhaseCores.Remove(core);
                destroyedCores--;
                count++;
            }
        }
    }

    private void SpawnSkillsAfterStun()
    {
        if (skillPrefabs.Count == 0 || spawnPoints.Count == 0) return;

        int spawnCount = Mathf.Min(spawnSkillCountPerStun, spawnPoints.Count);
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < spawnCount; i++)
        {
            int pointIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[pointIndex];
            availablePoints.RemoveAt(pointIndex);

            GameObject prefab = skillPrefabs[Random.Range(0, skillPrefabs.Count)];
            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    private void DestroyFinalCore()
    {
        isDead = true;

        if (stunCoroutine != null) { StopCoroutine(stunCoroutine); stunCoroutine = null; }

        onFinalCoreDestroyed?.Invoke();
        if (animator != null) animator.SetTrigger(dieTrigger);
        attackController?.OnBossDeath();

        if (deathDisableDelay > 0f) StartCoroutine(DoDeathAfterDelay(deathDisableDelay));
        else DoDeathImmediate();
    }

    private IEnumerator DoDeathAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DoDeathImmediate();
    }

    private void DoDeathImmediate()
    {
        onBossDie?.Invoke();
        enabled = false;
    }

    // ===========================
    // 手动触发、重置与调试
    // ===========================

    [ContextMenu("手动触发晕厥")]
    public void ManualTriggerStun()
    {
        if (isStunned) return;
        phaseTriggered = true;
        destroyedCores = phaseCores.Count;
        destroyedPhaseCores.Clear();
        destroyedPhaseCores.AddRange(phaseCores);
        EnterStunState();
    }

    [ContextMenu("添加所有阶段核心到重新激活列表")]
    public void AddAllPhaseCoresToReactivateList()
    {
        coresToReactivateAfterStun.Clear();
        coresToReactivateAfterStun.AddRange(phaseCores);
    }

    [ContextMenu("清空重新激活列表")]
    public void ClearReactivateList() => coresToReactivateAfterStun.Clear();

    [ContextMenu("打印所有检查点状态")]
    public void PrintAllCheckpointStates()
    {
        foreach (var state in recordedCheckpointStates)
        {
            Debug.Log($"📊 检查点 {state.checkpointPhaseIndex}: 破坏核心 {state.destroyedCoresCount}, 阶段触发 {state.phaseTriggered}");
        }
    }

    public void ForceTriggerPhase()
    {
        if (phaseTriggered) return;
        destroyedCores = phaseTriggerCount;
        TriggerPhaseChange();
    }

    public void ForceStun()
    {
        if (isStunned) return;
        destroyedCores = phaseCores.Count;
        destroyedPhaseCores.Clear();
        destroyedPhaseCores.AddRange(phaseCores);
        EnterStunState();
    }

    public void ForceDieNow()
    {
        if (isDead) return;
        if (finalCore != null && isStunned) DestroyFinalCore();
        else
        {
            isDead = true;
            destroyedCores = phaseCores.Count;
            if (deathDisableDelay > 0f) StartCoroutine(DoDeathAfterDelay(deathDisableDelay));
            else DoDeathImmediate();
        }
    }

    public void ResetBoss()
    {
        if (stunCoroutine != null) { StopCoroutine(stunCoroutine); stunCoroutine = null; }

        destroyedCores = 0;
        isDead = false;
        isStunned = false;
        stunLocked = false;
        phaseTriggered = false;
        destroyedPhaseCores.Clear();
        recordedCheckpointStates.Clear();

        bossFollower?.OnBossStunEnd();

        foreach (var core in phaseCores) core?.ResetCore();
        if (finalCore != null) finalCore.gameObject.SetActive(false);

        if (animator != null) { animator.Rebind(); animator.Update(0f); }

        enabled = true;
        RecordCheckpointState(0);
    }

    [ContextMenu("🧨 手动强制死亡 (测试用)")]
    public void ManualForceDeath()
    {
        if (isDead) return;
        if (isStunned && finalCore != null) { DestroyFinalCore(); return; }

        isDead = true;
        if (animator != null) animator.SetTrigger(dieTrigger);
        attackController?.OnBossDeath();

        if (deathDisableDelay > 0f) StartCoroutine(DoDeathAfterDelay(deathDisableDelay));
        else DoDeathImmediate();
    }

    private void ValidateReactivateCores()
    {
        for (int i = coresToReactivateAfterStun.Count - 1; i >= 0; i--)
        {
            var core = coresToReactivateAfterStun[i];
            if (core == null || !phaseCores.Contains(core)) coresToReactivateAfterStun.RemoveAt(i);
        }
    }

    public void RegisterPhaseCore(BossCore core)
    {
        if (!phaseCores.Contains(core)) phaseCores.Add(core);
    }
}
