using UnityEngine;
using System.Collections;

public class ToggleObjectsOnTrigger : MonoBehaviour
{
    [Header("延迟时间(秒)")]
    public float delay = 1f;

    [Header("渐变时间(秒)")]
    public float fadeDuration = 1f;

    [Header("需要切换的物体")]
    public GameObject[] targetObjects;

    [Header("触发一次后是否销毁该Trigger")]
    public bool destroyAfterTrigger = false;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //hasTriggered = true;
            StartCoroutine(ToggleAfterDelay());
        }
    }

    private IEnumerator ToggleAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                bool isActive = obj.activeSelf;

                if (!isActive)
                {
                    obj.SetActive(true); // 激活才能渐入
                    yield return StartCoroutine(FadeObject(obj, 0f, 1f, fadeDuration));
                }
                else
                {
                    yield return StartCoroutine(FadeObject(obj, 1f, 0f, fadeDuration));
                    obj.SetActive(false); // 渐出后关闭
                }
            }
        }

        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeObject(GameObject obj, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        // 优先检测 UI 的 CanvasGroup
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = startAlpha;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }
            cg.alpha = endAlpha;
        }
        else
        {
            // 处理 3D 模型 / Sprite
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                // 确保材质是独立实例
                r.material = new Material(r.material);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);

                foreach (Renderer r in renderers)
                {
                    if (r.material.HasProperty("_Color"))
                    {
                        Color c = r.material.color;
                        c.a = newAlpha;
                        r.material.color = c;
                    }
                }
                yield return null;
            }

            // 最终值修正
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = endAlpha;
                    r.material.color = c;
                }
            }
        }
    }
}
