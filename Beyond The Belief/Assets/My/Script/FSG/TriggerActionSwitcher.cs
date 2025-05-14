using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActionSwitcher: MonoBehaviour
{
    public string targetTag = "ActionTrigger";    // ��������ı�ǩ
    public string animationBoolName = "SitBool";  // Animator �еĴ���������

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("δ�ҵ� Animator �����");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            animator.SetBool(animationBoolName,true);
            Debug.Log("�����˶����л���");
        }
    }
}

