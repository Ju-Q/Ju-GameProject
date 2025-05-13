using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterToMainPage : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBoolTrue(bool istrue)
    {
        StartCoroutine(SetBoolsWithDelay(istrue));
    }

    private IEnumerator SetBoolsWithDelay(bool istrue)
    {
        animator.SetBool("ChapterTOmainpage", istrue);
        animator.SetBool("ChapterAppear", false); 

        // �ȴ�����
        yield return new WaitForSeconds(0);

        // �ڵȴ������ִ�����´���
        animator.SetBool("Cover_ButtomAppear", istrue);
        animator.SetBool("Cover_ButtomVanish", false);
        animator.SetBool("Cover_TitleVanish", false);
        animator.SetBool("Cover_TitleAppear", istrue);
        animator.SetBool("back_to_MainPage", istrue);
        animator.SetBool("chapter", false);
    }

    // Update is called once per frame
}
