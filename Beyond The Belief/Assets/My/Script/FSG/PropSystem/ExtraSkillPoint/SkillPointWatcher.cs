using UnityEngine;

public class SkillPointWatcher : MonoBehaviour
{
    [Header("引用")]
    public SkillPointManager skillPointManager;   // Skill Point Manager 脚本
    public Animator Animator;                     // 主角的 Animator
    public string[] validStates;                  // 允许的 Animator 状态名列表
    public GameObject targetObject;               // 条件成立时激活的物体
    public GameObject Canvas;                     // 条件成立时激活的 Canvas
    public DialogueManager1 dialogueManager;      // 新增：对话管理器

    [Header("区域检测")]
    public Collider triggerZone;                  // 特定区域 (可用 BoxCollider/其他)

    private bool lastConditionMet = false;       // 上一帧是否满足条件

    private void Update()
    {
        if (skillPointManager == null || Animator == null || dialogueManager == null) return;

        bool currentConditionMet = CheckConditions();

        // 条件第一次或再次满足时触发
        if (currentConditionMet && !lastConditionMet)
        {
            ActivateObjects();
            TriggerDialogue();
        }

        lastConditionMet = currentConditionMet;
    }

    private bool CheckConditions()
    {
        // Skill Points 为 0
        if (skillPointManager.currentSkillPoints != 0) return false;

        // 玩家在区域内
        if (!IsPlayerInsideZone()) return false;

        // Animator 状态正确
        if (!IsInValidAnimatorState()) return false;

        return true;
    }

    private void ActivateObjects()
    {
        if (targetObject != null && !targetObject.activeSelf)
            targetObject.SetActive(true);

        if (Canvas != null && !Canvas.activeSelf)
            Canvas.SetActive(true);
    }

    private void TriggerDialogue()
    {
        if (dialogueManager.allowRepeat)
        {
            // 允许重复时，直接调用触发方法
            dialogueManager.StartDialogueWithDelay(0f);
        }
        else
        {
            // 非重复模式，只触发一次
            if (!dialogueManager.isDialogueActive)
                dialogueManager.StartDialogueWithDelay(0f);
        }
    }

    private bool IsPlayerInsideZone()
    {
        if (triggerZone == null) return false;

        Collider[] colliders = Physics.OverlapBox(triggerZone.bounds.center, triggerZone.bounds.extents, triggerZone.transform.rotation);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
                return true;
        }
        return false;
    }

    private bool IsInValidAnimatorState()
    {
        foreach (string stateName in validStates)
        {
            if (Animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                return true;
        }
        return false;
    }
}
