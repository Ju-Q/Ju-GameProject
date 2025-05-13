using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Option_change : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCubeAni(bool istrue)
    {

        animator.SetBool("Cover_ButtomVanish", istrue);
        animator.SetBool("Cover_ButtomAppear", false);
        animator.SetBool("Cover_TitleVanish", istrue);
        animator.SetBool("Cover_TitleAppear", false);

        animator.SetBool("Option_change", istrue);
        animator.SetBool("opBackTo_MainPage", false);
        animator.SetBool("option", istrue);
        animator.SetBool("OP_back_to_MainPage", false);
    }
    // Update is called once per frame

}