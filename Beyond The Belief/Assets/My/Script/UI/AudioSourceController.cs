using UnityEngine;
using UnityEngine.UI; // 需要引入这个命名空间以使用Slider
using System.Collections;

public class AudioSourceController : MonoBehaviour
{
    public AudioSource audioSource; // 要激活的AudioSource
    public Slider volumeSlider; // 控制音量的Slider
    public float waitDuration = 5f; // 等待的秒数
    public float startVolume = 0f; // 初始音量
    public float targetVolume = 1f; // 目标音量
    public float changeDuration = 2f; // 音量变化的持续时间

    private void Start()
    {
        // 确保AudioSource和Slider已设置
        if (audioSource != null && volumeSlider != null)
        {
            audioSource.volume = startVolume; // 设置初始音量
            volumeSlider.value = startVolume; // 设置Slider初始值
            audioSource.Stop(); // 确保音频停止
            StartCoroutine(ActivateAudioSourceCoroutine());
        }
        else
        {
            Debug.LogError("AudioSource or Slider is not assigned!");
        }
    }

    private IEnumerator ActivateAudioSourceCoroutine()
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(waitDuration);

        // 激活AudioSource并播放
        audioSource.Play();
        Debug.Log("AudioSource activated and started playing.");

        // 渐变音量
        float elapsed = 0f;

        while (elapsed < changeDuration)
        {
            elapsed += Time.deltaTime;
            float currentVolume = Mathf.Lerp(startVolume, targetVolume, elapsed / changeDuration);
            audioSource.volume = currentVolume; // 更新AudioSource的音量
            volumeSlider.value = currentVolume; // 更新Slider的值
            yield return null; // 等待下一帧
        }

        audioSource.volume = targetVolume; // 确保音量达到目标值
        volumeSlider.value = targetVolume; // 确保Slider的值也更新
    }
}
