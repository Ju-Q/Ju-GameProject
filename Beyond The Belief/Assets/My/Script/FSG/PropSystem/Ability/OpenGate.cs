using UnityEngine;

public class OpenGate : MonoBehaviour
{
    [Header("设置要控制的Animator")]
    public Animator targetAnimator;

    [Header("Trigger参数名")]
    public string triggerParameterName = "Open";

    [Header("允许扣点的状态名称们")]
    public string[] allowedStates; // 可在 Inspector 里填多个状态名

    [Header("按键提示UI（Canvas）")]
    public GameObject promptCanvas;

    private bool isPlayerInTrigger = false;
    private bool hasConsumedSkillPoint = false; // 标记是否已经消耗过

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false); // 初始隐藏提示
        }
    }

    void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            if (targetAnimator != null && !string.IsNullOrEmpty(triggerParameterName))
            {
                AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);

                // ✅ 检查当前状态是否在 allowedStates 里
                bool isInAllowedState = false;
                foreach (string stateName in allowedStates)
                {
                    if (stateInfo.IsName(stateName))
                    {
                        isInAllowedState = true;
                        break;
                    }
                }

                if (isInAllowedState && !hasConsumedSkillPoint)
                {
                    if (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints > 0)
                    {
                        targetAnimator.SetTrigger(triggerParameterName);
                        SkillPointManager.Instance.UseSkillPoint();
                        hasConsumedSkillPoint = true; // ✅ 标记已扣
                        Debug.Log("触发Animator + 消耗技能点");

                        if (promptCanvas != null)
                            promptCanvas.SetActive(false);
                    }
                    else
                    {
                        Debug.Log("没有技能点，无法触发动画");
                    }
                }
                else
                {
                    Debug.Log("当前状态不允许触发 或 已经消耗过技能点");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            if (promptCanvas != null)
                promptCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            if (promptCanvas != null)
                promptCanvas.SetActive(false);
        }
    }
}
