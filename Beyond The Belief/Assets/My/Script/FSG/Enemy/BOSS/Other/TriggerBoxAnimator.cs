using UnityEngine;

public class TriggerBoxAnimator : MonoBehaviour
{
    [Header("要触发的Animator")]
    public Animator targetAnimator;

    [Header("Animator Trigger参数名")]
    public string triggerName = "PlayAnim";

    [Header("是否只触发一次")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetAnimator != null && !hasTriggered)
            {
                targetAnimator.SetTrigger(triggerName);

                if (triggerOnce)
                {
                    hasTriggered = true;
                }
            }
        }
    }
}
