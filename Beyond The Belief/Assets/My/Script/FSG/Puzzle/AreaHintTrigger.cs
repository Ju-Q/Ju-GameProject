using UnityEngine;
using System.Collections;

public class AreaHintTrigger : MonoBehaviour
{
    [Header("提示时间点(秒)")]
    public float[] hintTimes; // e.g. [30, 50, 80]

    [Header("提示UI（与时间点一一对应）")]
    public CanvasGroup[] hintUIs; // 注意这里要挂 CanvasGroup

    [Header("渐隐渐显设置")]
    public float fadeDuration = 1f; // 渐隐渐显时间
    public float displayDuration = 5f; // 每个提示显示多久后渐出

    private float stayTimer = 0f;
    private bool playerInside = false;
    private int currentHintIndex = 0;

    private void Start()
    {
        // 默认全部隐藏
        foreach (var ui in hintUIs)
        {
            if (ui != null)
            {
                ui.alpha = 0;
                ui.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerInside && currentHintIndex < hintTimes.Length)
        {
            stayTimer += Time.deltaTime;

            // 到达设定时间点 → 显示对应提示
            if (stayTimer >= hintTimes[currentHintIndex])
            {
                if (hintUIs[currentHintIndex] != null)
                {
                    StartCoroutine(ShowHint(hintUIs[currentHintIndex]));
                }

                currentHintIndex++; // 指向下一个提示
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            stayTimer = 0f;
            currentHintIndex = 0;

            // 重置 UI
            foreach (var ui in hintUIs)
            {
                if (ui != null)
                {
                    ui.alpha = 0;
                    ui.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            stayTimer = 0f;
            currentHintIndex = 0;

            // 离开时立刻渐出所有提示
            foreach (var ui in hintUIs)
            {
                if (ui != null && ui.gameObject.activeSelf)
                {
                    StartCoroutine(FadeCanvasGroup(ui, ui.alpha, 0f, fadeDuration));
                }
            }
        }
    }

    // 显示提示 → 渐入 + 停留 + 渐出
    private IEnumerator ShowHint(CanvasGroup cg)
    {
        yield return StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, fadeDuration)); // 渐入
        yield return new WaitForSeconds(displayDuration); // 停留
        yield return StartCoroutine(FadeCanvasGroup(cg, 1f, 0f, fadeDuration)); // 渐出
    }

    // 渐隐渐显协程
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        cg.gameObject.SetActive(true);
        cg.alpha = from;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        cg.alpha = to;

        if (Mathf.Approximately(to, 0f))
        {
            cg.gameObject.SetActive(false);
        }
    }
}
