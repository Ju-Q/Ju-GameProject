using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chapter_details1_vanish : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBoolTrue(bool istrue)
    {
        animator.SetBool("detail_vanish", istrue);
        animator.SetBool("detail", false);
        animator.SetBool("1to2", false);
        animator.SetBool("2to1", false);
        animator.SetBool("detail2", false);
    }
    // Update is called once per frame

}