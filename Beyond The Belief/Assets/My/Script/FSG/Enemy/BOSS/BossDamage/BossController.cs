using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class BossController : MonoBehaviour
{
    [Header("Animator (可拖拽，空则自动查找子物体)")]
    public Animator animator;
    [Tooltip("每次核心被破坏触发的Trigger名")]
    public string hitTrigger = "Hit";
    [Tooltip("全部核心被破坏触发的Trigger名")]
    public string dieTrigger = "Die";
    [Tooltip("晕厥状态Trigger名")]
    public string stunTrigger = "Stun";
    [Tooltip("醒来状态Trigger名")]
    public string wakeUpTrigger = "WakeUp";

    [Header("EnemyAttackController (可拖拽，空则自动查找)")]
    public EnemyAttackController attackController;

    [Header("Boss 核心设置")]
    public List<BossCore> phaseCores = new List<BossCore>(); // 阶段核心
    public BossCore finalCore; // 最终核心（独立）

    [Header("阶段设置")]
    [Tooltip("触发场景变化的破坏核心数量")]
    public int phaseTriggerCount = 3;
    [Tooltip("晕厥持续时间（秒）")]
    public float stunDuration = 10f;
    [Tooltip("晕厥醒来后要重新激活的特定核心（可拖拽指定）")]
    public List<BossCore> coresToReactivateAfterStun = new List<BossCore>(); // 修改：改为指定具体核心

    [Header("BossFollowPlayer引用")]
    [Tooltip("手动拖拽BossFollowPlayer所在的物体")]
    public BossFollowPlayer bossFollowerManual;

    [Header("死亡设置")]
    [Tooltip("在触发死亡后延迟多少秒执行最终死亡逻辑（可用于等动画播完）")]
    public float deathDisableDelay = 0f;
    [Tooltip("在 Inspector 可以绑定死亡时要执行的动作（掉落、禁用AI等）")]
    public UnityEvent onBossDie;

    [Header("阶段事件")]
    [Tooltip("当达到指定破坏数量时触发场景变化")]
    public UnityEvent onPhaseTrigger;
    [Tooltip("当晕厥结束时触发（恢复场景）")]
    public UnityEvent onPhaseReset;
    [Tooltip("当最终核心被破坏时触发")]
    public UnityEvent onFinalCoreDestroyed;

    // 状态变量
    private int destroyedCores = 0;
    private bool isDead = false;
    private bool isStunned = false;
    private bool phaseTriggered = false;
    private Coroutine stunCoroutine;
    private BossFollowPlayer bossFollower;

    // 跟踪已被破坏的核心
    private List<BossCore> destroyedPhaseCores = new List<BossCore>();

    // 公共属性
    public int DestroyedCoresCount => destroyedCores;
    public bool IsStunned => isStunned;
    public bool PhaseTriggered => phaseTriggered;
    public bool IsDead => isDead;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (attackController == null)
            attackController = GetComponent<EnemyAttackController>();

        // 优先使用手动设置的引用
        if (bossFollowerManual != null)
        {
            bossFollower = bossFollowerManual;
            Debug.Log($"✅ 使用手动设置的BossFollowPlayer: {bossFollower.gameObject.name}");
        }
        else
        {
            // 尝试自动获取（在同一物体或子物体中）
            bossFollower = GetComponent<BossFollowPlayer>();
            if (bossFollower == null)
                bossFollower = GetComponentInChildren<BossFollowPlayer>();

            if (bossFollower != null)
            {
                Debug.Log($"✅ 自动找到BossFollowPlayer: {bossFollower.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ 未找到BossFollowPlayer，请在Inspector中手动设置bossFollowerManual");
            }
        }

        // 自动收集阶段核心（排除最终核心）
        if (phaseCores == null || phaseCores.Count == 0)
        {
            var found = GetComponentsInChildren<BossCore>(true);
            foreach (var c in found)
            {
                if (c != null && c != finalCore && !c.isFinalCore && !phaseCores.Contains(c))
                    phaseCores.Add(c);
            }
        }

        // 验证指定的重新激活核心是否在阶段核心列表中
        ValidateReactivateCores();

        // 隐藏最终核心（如果存在）
        if (finalCore != null)
            finalCore.gameObject.SetActive(false);

        destroyedCores = 0;
        isDead = false;
        isStunned = false;
        phaseTriggered = false;
        destroyedPhaseCores.Clear();
    }

    private void Start()
    {
        Debug.Log("🔍 BossController初始化完成:");
        Debug.Log($"   所在物体: {gameObject.name}");
        Debug.Log($"   BossFollowPlayer: {(bossFollower != null ? bossFollower.gameObject.name : "NULL")}");
        Debug.Log($"   阶段核心数量: {phaseCores.Count}");
        Debug.Log($"   最终核心: {(finalCore != null ? finalCore.gameObject.name : "NULL")}");
        Debug.Log($"   晕厥后重新激活的核心数量: {coresToReactivateAfterStun.Count}");
    }

    // 新增：验证指定的重新激活核心是否有效
    private void ValidateReactivateCores()
    {
        for (int i = coresToReactivateAfterStun.Count - 1; i >= 0; i--)
        {
            BossCore core = coresToReactivateAfterStun[i];
            if (core == null || !phaseCores.Contains(core))
            {
                Debug.LogWarning($"⚠️ 指定的重新激活核心无效或不在阶段核心列表中: {core?.name}，已从列表中移除");
                coresToReactivateAfterStun.RemoveAt(i);
            }
        }
    }

    public void RegisterPhaseCore(BossCore core)
    {
        if (!phaseCores.Contains(core))
            phaseCores.Add(core);
    }

    public void CoreDestroyed(BossCore core)
    {
        if (isDead) return;

        Debug.Log($"🔨 CoreDestroyed 被调用: {core?.name}");
        Debug.Log($"   当前 destroyedCores: {destroyedCores}");

        // 检查是否是最终核心
        if (core == finalCore && finalCore != null)
        {
            Debug.Log("🎯 最终核心被破坏");
            DestroyFinalCore();
            return;
        }

        // 阶段核心破坏逻辑
        if (!phaseCores.Contains(core))
        {
            Debug.Log("❌ 核心不在phaseCores列表中");
            return;
        }

        destroyedCores++;

        // 记录被破坏的核心
        if (!destroyedPhaseCores.Contains(core))
            destroyedPhaseCores.Add(core);

        Debug.Log($"📊 核心计数更新: {destroyedCores}/{phaseCores.Count}");

        if (animator != null)
            animator.SetTrigger(hitTrigger);

        // 检查是否触发阶段变化
        if (!phaseTriggered && destroyedCores >= phaseTriggerCount)
        {
            Debug.Log($"🚀 触发阶段变化条件满足");
            TriggerPhaseChange();
        }

        // 检查是否所有阶段核心都被破坏（进入晕厥）
        Debug.Log($"🔍 检查晕厥条件: phaseTriggered={phaseTriggered}, destroyedCores={destroyedCores}, phaseCores.Count={phaseCores.Count}");
        if (phaseTriggered && destroyedCores >= phaseCores.Count)
        {
            Debug.Log("🎯 晕厥条件满足，调用 EnterStunState");
            EnterStunState();
        }
        else
        {
            Debug.Log("❌ 晕厥条件不满足");
        }
    }

    private void TriggerPhaseChange()
    {
        phaseTriggered = true;
        onPhaseTrigger?.Invoke();
        Debug.Log($"阶段触发！已破坏 {destroyedCores} 个核心，触发场景变化");
    }

    private void EnterStunState()
    {
        if (isStunned) return;

        isStunned = true;
        Debug.Log($"🎯 BossController.EnterStunState 被调用");

        // 播放晕厥动画
        if (animator != null)
            animator.SetTrigger(stunTrigger);

        // 检查bossFollower引用
        if (bossFollower == null)
        {
            Debug.LogError("❌ bossFollower 为 null，尝试重新获取");
            if (bossFollowerManual != null)
            {
                bossFollower = bossFollowerManual;
                Debug.Log($"✅ 重新使用手动设置的BossFollowPlayer: {bossFollower.gameObject.name}");
            }
        }

        if (bossFollower != null)
        {
            Debug.Log("🔄 准备调用 bossFollower.OnBossStunStart()");
            bossFollower.OnBossStunStart();
        }
        else
        {
            Debug.LogError("❌ 无法调用 OnBossStunStart，bossFollower 为空");
        }

        // 显示最终核心
        if (finalCore != null)
        {
            finalCore.gameObject.SetActive(true);
            finalCore.ResetCore();
        }

        // 停止攻击
        if (attackController != null)
            attackController.OnBossStun();

        // 开始晕厥计时
        stunCoroutine = StartCoroutine(StunCountdown());

        Debug.Log("Boss进入晕厥状态！");
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
        Debug.Log("🔄 BossController.WakeUpBoss 被调用");

        // 停止晕厥协程
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        // 通知BossFollowPlayer晕厥结束
        if (bossFollower != null)
        {
            bossFollower.OnBossStunEnd();
        }

        // 隐藏最终核心
        if (finalCore != null)
            finalCore.gameObject.SetActive(false);

        // 重新激活指定的核心
        ReactivateSpecificCoresAfterStun();

        // 播放醒来动画
        if (animator != null)
            animator.SetTrigger(wakeUpTrigger);

        // 恢复攻击
        if (attackController != null)
            attackController.OnBossWakeUp();

        // 触发场景恢复
        onPhaseReset?.Invoke();

        Debug.Log("Boss醒来！");
    }

    // 修改：重新激活指定的核心
    private void ReactivateSpecificCoresAfterStun()
    {
        Debug.Log($"🔄 准备重新激活指定的 {coresToReactivateAfterStun.Count} 个核心");

        int successfullyReactivated = 0;

        foreach (BossCore core in coresToReactivateAfterStun)
        {
            if (core != null && destroyedPhaseCores.Contains(core))
            {
                // 重新激活核心
                core.ResetCore();
                core.gameObject.SetActive(true);

                // 从已破坏列表中移除
                destroyedPhaseCores.Remove(core);

                // 减少破坏计数
                destroyedCores--;
                successfullyReactivated++;

                Debug.Log($"✅ 重新激活核心: {core.name}");
            }
            else if (core != null)
            {
                Debug.Log($"⚠️ 核心 {core.name} 未被破坏或不存在于已破坏列表中，跳过重新激活");
            }
        }

        Debug.Log($"✅ 成功重新激活了 {successfullyReactivated} 个指定核心");
        Debug.Log($"   剩余已破坏核心: {destroyedPhaseCores.Count}");
        Debug.Log($"   当前破坏计数: {destroyedCores}");
    }

    private void DestroyFinalCore()
    {
        isDead = true;
        Debug.Log("💀 DestroyFinalCore 被调用，Boss真正死亡");

        // 停止所有协程
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        // 触发最终核心破坏事件
        onFinalCoreDestroyed?.Invoke();

        // 播放死亡动画
        if (animator != null)
            animator.SetTrigger(dieTrigger);

        // 通知攻击控制器
        if (attackController != null)
            attackController.OnBossDeath();

        // 延迟执行死亡逻辑
        if (deathDisableDelay > 0f)
            StartCoroutine(DoDeathAfterDelay(deathDisableDelay));
        else
            DoDeathImmediate();

        Debug.Log("最终核心被破坏！Boss死亡！");
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

    // 调试方法
    [ContextMenu("手动触发晕厥")]
    public void ManualTriggerStun()
    {
        if (isStunned) return;

        // 强制设置条件
        phaseTriggered = true;
        destroyedCores = phaseCores.Count;

        // 模拟记录所有核心为已破坏
        destroyedPhaseCores.Clear();
        destroyedPhaseCores.AddRange(phaseCores);

        EnterStunState();
    }

    // 新增：在Inspector中添加指定核心的便捷方法
    [ContextMenu("添加所有阶段核心到重新激活列表")]
    public void AddAllPhaseCoresToReactivateList()
    {
        coresToReactivateAfterStun.Clear();
        coresToReactivateAfterStun.AddRange(phaseCores);
        Debug.Log($"✅ 已添加所有 {phaseCores.Count} 个阶段核心到重新激活列表");
    }

    [ContextMenu("清空重新激活列表")]
    public void ClearReactivateList()
    {
        coresToReactivateAfterStun.Clear();
        Debug.Log("✅ 已清空重新激活列表");
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

        // 模拟记录所有核心为已破坏
        destroyedPhaseCores.Clear();
        destroyedPhaseCores.AddRange(phaseCores);

        EnterStunState();
    }

    public void ForceDieNow()
    {
        if (isDead) return;

        if (finalCore != null && isStunned)
        {
            DestroyFinalCore();
        }
        else
        {
            isDead = true;
            destroyedCores = phaseCores.Count;
            if (deathDisableDelay > 0f)
                StartCoroutine(DoDeathAfterDelay(deathDisableDelay));
            else
                DoDeathImmediate();
        }
    }

    // 重置Boss状态（用于Boss重生）
    public void ResetBoss()
    {
        // 停止所有协程
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        // 重置状态变量
        destroyedCores = 0;
        isDead = false;
        isStunned = false;
        phaseTriggered = false;
        destroyedPhaseCores.Clear();

        // 通知BossFollowPlayer恢复正常
        if (bossFollower != null)
        {
            bossFollower.OnBossStunEnd();
        }

        // 重置所有阶段核心
        foreach (var core in phaseCores)
        {
            if (core != null)
                core.ResetCore();
        }

        // 隐藏最终核心
        if (finalCore != null)
            finalCore.gameObject.SetActive(false);

        // 重置动画状态
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // 重新启用脚本
        enabled = true;

        Debug.Log("Boss状态已重置");
    }
}