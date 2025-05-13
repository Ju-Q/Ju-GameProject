using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class option_to_MainPage : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCubeAni(bool istrue)
    {

        animator.SetBool("Cover_ButtomVanish", false);
        animator.SetBool("Cover_ButtomAppear", istrue);
        animator.SetBool("Cover_TitleVanish", false);
        animator.SetBool("Cover_TitleAppear", istrue);

        animator.SetBool("Option_change", false);
        animator.SetBool("opBackTo_MainPage", istrue);
        animator.SetBool("option", false);
        animator.SetBool("OP_back_to_MainPage", istrue);
    }
    // Update is called once per frame

}