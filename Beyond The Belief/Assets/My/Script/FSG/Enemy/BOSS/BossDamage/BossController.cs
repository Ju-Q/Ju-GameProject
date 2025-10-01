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

    [Header("EnemyAttackController (可拖拽，空则自动查找)")]
    public EnemyAttackController attackController;

    [Header("Boss 核心（可在 Inspector 手动填，留空则自动收集子物体上的 BossCore）")]
    public List<BossCore> cores = new List<BossCore>();

    [Header("死亡设置")]
    [Tooltip("在触发死亡后延迟多少秒执行最终死亡逻辑（可用于等动画播完）")]
    public float deathDisableDelay = 0f;

    [Tooltip("在 Inspector 可以绑定死亡时要执行的动作（掉落、禁用AI等）")]
    public UnityEvent onBossDie;

    private int destroyedCores = 0;
    private bool isDead = false;

    // ✅ 新增：给 EnemyAttackController 用的
    public int DestroyedCoresCount => destroyedCores;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (attackController == null)
            attackController = GetComponent<EnemyAttackController>();

        if (cores == null || cores.Count == 0)
        {
            var found = GetComponentsInChildren<BossCore>(true);
            foreach (var c in found)
            {
                if (c != null && !cores.Contains(c))
                    cores.Add(c);
            }
        }

        destroyedCores = 0;
        isDead = false;
    }

    public void RegisterCore(BossCore core)
    {
        if (!cores.Contains(core))
            cores.Add(core);
    }

    public void CoreDestroyed(BossCore core)
    {
        if (isDead) return;

        destroyedCores++;

        if (animator != null)
            animator.SetTrigger(hitTrigger);

        int total = Mathf.Max(1, cores.Count);
        if (destroyedCores >= total)
        {
            isDead = true;

            if (animator != null)
                animator.SetTrigger(dieTrigger);

            if (attackController == null)
                attackController = GetComponent<EnemyAttackController>();

            if (attackController != null)
                attackController.OnBossDeath();

            if (deathDisableDelay > 0f)
                StartCoroutine(DoDeathAfterDelay(deathDisableDelay));
            else
                DoDeathImmediate();
        }
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

    public void ForceDieNow()
    {
        if (isDead) return;
        destroyedCores = Mathf.Max(1, cores.Count);
        CoreDestroyed(null);
    }
}
