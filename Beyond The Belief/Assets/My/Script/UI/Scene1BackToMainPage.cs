using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene1BackToMainPage : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCubeAni(bool istrue)
    {
        animator.SetBool("BacktoMainPage", istrue);
        animator.SetBool("Scene1_bgmVanish", istrue);
        //animator.SetBool("Cover_TitleVanish", istrue);
    }
    // Update is called once per frame

}