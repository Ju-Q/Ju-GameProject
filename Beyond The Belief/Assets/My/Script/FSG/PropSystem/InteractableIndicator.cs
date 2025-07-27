using UnityEngine;
using System.Collections;

public class InteractableIndicator : MonoBehaviour
{
    [Header("消失对象")]
    public GameObject indicatorGameObject;

    [Header("动画配置")]
    public Animator targetAnimator;        // 控制动画的 Animator
    public string triggerName = "Hide";    // Animator 中的 Trigger 名称
    public float hideDelay = 1f;           // 延迟时间（秒）

    private bool isHiding = false;

    private void Start()
    {
        if (indicatorGameObject != null)
        {
            indicatorGameObject.SetActive(false);
        }
    }

    public void ShowIndicator()
    {
        if (indicatorGameObject != null)
            indicatorGameObject.SetActive(true);
    }

    public void HideIndicator()
    {
        if (!isHiding && indicatorGameObject != null)
        {
            StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        isHiding = true;

        // 触发动画
        if (targetAnimator != null && !string.IsNullOrEmpty(triggerName))
        {
            targetAnimator.SetTrigger(triggerName);
        }

        // 等待指定秒数
        yield return new WaitForSeconds(hideDelay);

        // 隐藏提示UI
        indicatorGameObject.SetActive(false);
        isHiding = false;
    }
}
