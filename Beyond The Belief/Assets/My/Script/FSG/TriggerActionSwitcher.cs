using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActionSwitcher: MonoBehaviour
{
    public string targetTag = "ActionTrigger";    // 触发区域的标签
    public string animationBoolName = "SitBool";  // Animator 中的触发器参数

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("未找到 Animator 组件！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            animator.SetBool(animationBoolName,true);
            Debug.Log("触发了动作切换！");
        }
    }
}

