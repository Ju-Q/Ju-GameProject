using System.Collections;
using UnityEngine;
using StarterAssets;

public class DialogueManager2 : MonoBehaviour
{
    public GameObject ShowCanvas; // 用于显示对话的 Canvas
    private bool isCanvasActive = false;
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

    // ❌ 删除立即关闭逻辑
    // private void OnTriggerExit(Collider other) { ... }

    private IEnumerator ShowDialogue()
    {
        hasDialogueBeenShown = true;
        isCanvasActive = true;
        ShowCanvas.SetActive(true);

        // 调用淡入效果
        yield return StartCoroutine(FadeInCanvas());

        // 等待指定秒数后关闭
        yield return new WaitForSeconds(showCanvasTime);

        yield return StartCoroutine(FadeOutCanvas());
    }

    private IEnumerator FadeInCanvas()
    {
        float fadeDuration = 0.1f;
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutCanvas()
    {
        float fadeDuration = 0.1f;
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>();
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
