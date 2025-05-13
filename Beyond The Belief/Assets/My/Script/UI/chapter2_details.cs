using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chapter2_details : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBoolTrue(bool istrue)
    {
        animator.SetBool("detail2", istrue);
        animator.SetBool("detail2_vanish", false);
        animator.SetBool("detail_vanish", false);
     
    }
    // Update is called once per frame

}
