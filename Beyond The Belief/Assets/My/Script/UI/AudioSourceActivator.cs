using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceActivator : MonoBehaviour
{
    public AudioSource audioSource; // 要激活的AudioSource
    public float delayInSeconds = 5f; // 延迟的秒数

    private void Start()
    {
        // 确保AudioSource是未激活状态
        if (audioSource != null)
        {
            audioSource.Stop();
            //audioSource.volume = 0; // 可选：设置音量为0以避免听到声音
            Debug.Log("Starting coroutine to activate audio source.");
            StartCoroutine(ActivateAudioSource());
        }
        else
        {
            Debug.LogError("AudioSource is not assigned!");
        }
    }

    private IEnumerator ActivateAudioSource()
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(delayInSeconds);

        // 激活AudioSource
        if (audioSource != null)
        {
            audioSource.volume = 1; // 恢复音量
            audioSource.Play(); // 播放音频
            Debug.Log("AudioSource activated and playing.");
        }
        else
        {
            Debug.LogError("AudioSource is null after delay.");
        }
    }
}
