using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changescene : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCubeAni(bool istrue)
    {
        animator.SetBool("go", istrue);
        animator.SetBool("Cover_ButtomVanish", istrue);
        animator.SetBool("Cover_TitleVanish", istrue);
        animator.SetBool("Cover_TitleAppear", false);
        animator.SetBool("Cover_ButtomAppear", false);
    }
        // Update is called once per frame
 
}
