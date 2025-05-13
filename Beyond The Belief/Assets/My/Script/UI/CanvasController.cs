using System.Collections;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject canvasToActivate; // 要激活的 Canvas
    public Animator animator; // 用于动画控制的 Animator
    public string openTrigger; // 打开时的触发器名称
    public string closeTrigger; // 关闭时的触发器名称
    public AudioSource audioSource; // 音频源
    public AudioClip openSound; // 打开时播放的音频
    public AudioClip closeSound; // 关闭时播放的音频

    private bool isCanvasActive = false; // Canvas 当前状态
    private int sortingOrder = 100; // 确保 Canvas 在最上层的排序值

    void Update()
    {
        // 检测 F1 键是否被按下
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleCanvas();
        }
    }

    void ToggleCanvas()
    {
        // 切换 Canvas 的激活状态
        isCanvasActive = !isCanvasActive;

        if (isCanvasActive)
        {
            // 激活 Canvas
            canvasToActivate.SetActive(true);
            // 确保 Canvas 在最前面
            Canvas canvas = canvasToActivate.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder; // 设置排序层级
            }
            animator.SetTrigger(openTrigger); // 播放打开动画

            // 播放打开音频
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // 显示光标
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // 解锁光标
        }
        else
        {
            // 关闭 Canvas 并触发关闭动画
            animator.SetTrigger(closeTrigger); // 播放关闭动画

            // 播放关闭音频
            if (audioSource != null && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }

            StartCoroutine(DeactivateCanvasAfterAnimation());
        }
    }

    private IEnumerator DeactivateCanvasAfterAnimation()
    {
        // 等待 Animator 播放完关闭动画
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        canvasToActivate.SetActive(false); // 关闭 Canvas

        // 隐藏光标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // 锁定光标
    }
}
