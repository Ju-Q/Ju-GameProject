using UnityEngine;

public class OpenGate : MonoBehaviour
{
    [Header("设置要控制的Animator")]
    public Animator targetAnimator;

    [Header("Trigger参数名")]
    public string triggerParameterName = "Open";

    [Header("允许扣点的状态名称们")]
    public string[] allowedStates;

    [Header("按键提示UI（Canvas）")]
    public GameObject promptCanvas;

    private bool isPlayerInTrigger = false;
    public bool hasConsumedSkillPoint = false;

    void Start()
    {
        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInTrigger)
        {
            CheckCanTrigger(); // 每帧检查当前是否满足触发条件

            if (Input.GetKeyDown(KeyCode.F))
            {
                TryTriggerAnimator();
            }
        }
    }

    private void TryTriggerAnimator()
    {
        if (targetAnimator == null || string.IsNullOrEmpty(triggerParameterName))
        {
            Debug.LogWarning("Animator 或 Trigger 名未设置！");
            return;
        }

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);

        bool isInAllowedState = false;
        foreach (string stateName in allowedStates)
        {
            if (stateInfo.IsName(stateName))
            {
                isInAllowedState = true;
                break;
            }
        }

        if (!isInAllowedState)
        {
            Debug.Log("当前状态不允许触发动画: " + stateInfo.fullPathHash);
            return;
        }

        if (hasConsumedSkillPoint)
        {
            Debug.Log("技能点已经消耗过，无法再次触发");
            return;
        }

        if (SkillPointManager.Instance == null || SkillPointManager.Instance.currentSkillPoints <= 0)
        {
            Debug.Log("没有技能点，无法触发动画");
            return;
        }

        // ✅ 触发动画
        targetAnimator.SetTrigger(triggerParameterName);
        SkillPointManager.Instance.UseSkillPoint();
        hasConsumedSkillPoint = true;
        Debug.Log("✅ 动画触发成功，并消耗技能点");

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void CheckCanTrigger()
    {
        if (targetAnimator == null || string.IsNullOrEmpty(triggerParameterName))
            return;

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
        bool isInAllowedState = false;
        foreach (string stateName in allowedStates)
        {
            if (stateInfo.IsName(stateName))
            {
                isInAllowedState = true;
                break;
            }
        }

        string debugMsg = "状态: " + stateInfo.fullPathHash +
                          " | 在允许列表中: " + isInAllowedState +
                          " | 技能点可用: " +
                          (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints > 0) +
                          " | 已消耗: " + hasConsumedSkillPoint;

        Debug.Log(debugMsg);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            if (promptCanvas != null)
                promptCanvas.SetActive(true);

            Debug.Log("玩家进入触发区");
            CheckCanTrigger(); // 玩家进入时打印状态
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            if (promptCanvas != null)
                promptCanvas.SetActive(false);

            Debug.Log("玩家离开触发区");
        }
    }
}
