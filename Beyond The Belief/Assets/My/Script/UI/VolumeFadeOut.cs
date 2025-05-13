using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VolumeFadeOut : MonoBehaviour
{
    public AudioSource audioSource; // 要控制的AudioSource
    public Button button; // 触发音量降低的按钮
    public float waitDuration = 2f; // 等待的秒数
    public float fadeDuration = 2f; // 降到0的时间

    private void Start()
    {
        // 确保按钮点击事件已绑定
        if (button != null)
        {
            button.onClick.AddListener(StartFadeOutProcess);
        }
        else
        {
            Debug.LogError("Button is not assigned!");
        }
    }

    private void StartFadeOutProcess()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(waitDuration);

        float startVolume = audioSource.volume; // 获取当前音量
        float elapsed = 0f;

        // 渐渐降低音量
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeDuration);
            yield return null; // 等待下一帧
        }

        audioSource.volume = 0; // 确保音量完全降到0
        audioSource.Stop(); // 可选：停止音频播放
    }
}
