using System.Collections;
using UnityEngine;

public class DialogueManager2 : MonoBehaviour
{
    public GameObject ShowCanvas; // 用于显示对话的 Canvas
    private bool isCanvasActive = false;
    private bool isWaitingForInput = false;
    private bool hasDialogueBeenShown = false; // 记录对话是否已经显示过
    public float showCanvasTime = 5f;

    private void Start()
    {
        ShowCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasDialogueBeenShown)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isCanvasActive)
        {
            StopAllCoroutines(); // 确保在离开区域时停止任何进行中的对话显示
            StartCoroutine(FadeOutCanvas());
        }
    }

    private IEnumerator ShowDialogue()
    {
        hasDialogueBeenShown = true; // 标记对话已经显示过
        isCanvasActive = true;
        ShowCanvas.SetActive(true);

        // 调用淡入效果
        yield return StartCoroutine(FadeInCanvas());

        isWaitingForInput = false;

        // 假设显示对话的逻辑（这里只显示提示文字或其他 UI 元素）
        yield return new WaitForSeconds(showCanvasTime); // 假设对话停留 5 秒

        isWaitingForInput = true;

        while (isWaitingForInput)
        {
            if (Input.GetKeyDown(KeyCode.Return)) // 检测回车键
            {
                StartCoroutine(FadeOutCanvas());
                yield break;
            }
            yield return null; // 等待下一帧
        }
    }

    private IEnumerator FadeInCanvas()
    {
        float fadeDuration = 0.1f; // 渐变时间
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>(); // 如果没有 CanvasGroup 组件，添加一个
            canvasGroup.alpha = 0f; // 初始化为完全透明
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f; // 确保最后的透明度设置为完全不透明
    }

    private IEnumerator FadeOutCanvas()
    {
        float fadeDuration = 0.1f; // 渐变时间
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>(); // 如果没有 CanvasGroup 组件，添加一个
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        ShowCanvas.SetActive(false);
        isCanvasActive = false;
    }
}
