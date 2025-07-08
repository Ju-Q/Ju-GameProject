using UnityEngine;

public class OpenGate : MonoBehaviour
{
    [Header("设置要控制的Animator")]
    public Animator targetAnimator;

    [Header("Trigger参数名")]
    public string triggerParameterName = "Open";

    private bool isPlayerInTrigger = false;

    void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            if (targetAnimator != null && !string.IsNullOrEmpty(triggerParameterName))
            {
                // 如果有技能点才允许触发动画
                if (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints > 0)
                {
                    targetAnimator.SetTrigger(triggerParameterName);
                    SkillPointManager.Instance.UseSkillPoint(); // 消耗一个技能点
                    Debug.Log("触发Animator + 消耗技能点");
                }
                else
                {
                    Debug.Log("没有技能点，无法触发动画");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }
}
