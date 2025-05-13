using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chapter_change : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCubeAni(bool istrue)
    {
        animator.SetBool("chapter", istrue);
        animator.SetBool("Cover_ButtomVanish", istrue);
        animator.SetBool("Cover_ButtomAppear", false);
        animator.SetBool("Cover_TitleVanish", istrue);
        animator.SetBool("Cover_TitleAppear", false);
        animator.SetBool("ChapterAppear", istrue);
        animator.SetBool("anti_falseContact", istrue);
        animator.SetBool("ChapterTOmainpage", false);
        animator.SetBool("back_to_MainPage", false);
    }
    // Update is called once per frame

}