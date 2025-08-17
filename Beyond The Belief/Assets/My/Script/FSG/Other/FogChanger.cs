using UnityEngine;
using System.Collections;

public class FogChanger : MonoBehaviour
{
    public float targetFogDensity = 0.05f; // 目标雾浓度
    public bool enableFog = true; // 是否启用雾
    public float changeDuration = 2f; // 变化时间（秒）

    private Coroutine fogCoroutine; // 用来防止协程重复运行

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.fog = enableFog; // 开启或关闭雾

            // 如果已经在运行一个渐变协程，先停止
            if (fogCoroutine != null)
                StopCoroutine(fogCoroutine);

            fogCoroutine = StartCoroutine(ChangeFogDensitySmooth(targetFogDensity, changeDuration));
        }
    }

    private IEnumerator ChangeFogDensitySmooth(float target, float duration)
    {
        float startDensity = RenderSettings.fogDensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, target, elapsed / duration);
            yield return null;
        }

        RenderSettings.fogDensity = target; // 确保最终值准确
    }
}
