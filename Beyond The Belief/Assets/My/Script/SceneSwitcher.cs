using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public Animator animator; // ָ���ж����� Animator
    public string sceneToLoad; // ��Ҫ���صĳ�������
    public string animationToWaitFor; // ��Ҫ�ȴ��Ķ�������
    public float waitTime = 1f; // Ĭ�ϵȴ���ʱ�䣨��ѡ��

    public void StartSceneSwitch()
    {
        StartCoroutine(SwitchSceneAfterAnimation());
    }

    private IEnumerator SwitchSceneAfterAnimation()
    {
        // ����ָ������
        animator.SetTrigger(animationToWaitFor); // ����Ը�����Ҫ���ô�����

        // �ȴ�������ɣ�ʹ�� Animator ��״̬����ʱ��
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = stateInfo.length; // ��ȡ��ǰ������ʱ��

        // �����Ҫ������Լ���һ��С���ӳ٣�ȷ��������ȷ������
        yield return new WaitForSeconds(animationDuration + waitTime);

        // �л�����
        SceneManager.LoadScene(sceneToLoad);
    }
}

