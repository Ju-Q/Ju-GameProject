using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TriggerCanvasActivator : MonoBehaviour
{
    [Header("要激活的Canvas")]
    public GameObject canvasObject;

    [Header("渐变设置")]
    public float fadeDuration = 1f; // 渐变时间

    private CanvasGroup canvasGroup;
    private Coroutine currentFadeRoutine;
    private bool isPlayerInside = false;
    private bool isCanvasVisible = false;

    private void Start()
    {
        if (canvasObject != null)
        {
            canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvasObject.AddComponent<CanvasGroup>();
            }

            canvasObject.SetActive(false);
            canvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        if (isPlayerInside && isCanvasVisible && Input.GetKeyDown(KeyCode.F))
        {
            // 玩家在区域内且按下 F，淡出并关闭 Canvas
            StartFade(0f, () =>
            {
                canvasObject.SetActive(false);
                isCanvasVisible = false;
            });
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (canvasObject != null)
            {
                canvasObject.SetActive(true);
                StartFade(1f, () =>
                {
                    isCanvasVisible = true;
                });
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (canvasObject != null)
            {
                StartFade(0f, () =>
                {
                    canvasObject.SetActive(false);
                    isCanvasVisible = false;
                });
            }
        }
    }

    private void StartFade(float targetAlpha, System.Action onComplete = null)
    {
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeCanvas(targetAlpha, onComplete));
    }

    private IEnumerator FadeCanvas(float targetAlpha, System.Action onComplete)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}
