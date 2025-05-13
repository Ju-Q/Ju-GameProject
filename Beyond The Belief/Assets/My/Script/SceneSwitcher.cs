using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public Animator animator; // 指向含有动画的 Animator
    public string sceneToLoad; // 需要加载的场景名称
    public string animationToWaitFor; // 需要等待的动画名称
    public float waitTime = 1f; // 默认等待的时间（可选）

    public void StartSceneSwitch()
    {
        StartCoroutine(SwitchSceneAfterAnimation());
    }

    private IEnumerator SwitchSceneAfterAnimation()
    {
        // 播放指定动画
        animator.SetTrigger(animationToWaitFor); // 你可以根据需要设置触发器

        // 等待动画完成，使用 Animator 的状态持续时间
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = stateInfo.length; // 获取当前动画的时长

        // 如果需要，你可以加上一个小的延迟，确保动画的确播放完
        yield return new WaitForSeconds(animationDuration + waitTime);

        // 切换场景
        SceneManager.LoadScene(sceneToLoad);
    }
}

