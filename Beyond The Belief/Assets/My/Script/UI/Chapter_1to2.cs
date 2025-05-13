using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chapter_1to2 : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayCubeAni(bool istrue)
    {
        animator.SetBool("1to2", istrue);
        animator.SetBool("2to1", false);
        //animator.SetBool("Cover_ButtomVanish", istrue);
        //animator.SetBool("Cover_TitleVanish", istrue);
        //animator.SetBool("ChapterAppear", istrue);
    }
    // Update is called once per frame

}
