using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间以使用Button

public class PlayAudioOnButtonPress : MonoBehaviour
{
    public AudioSource audioSource; // 要播放的AudioSource
    public Button playButton; // 按钮，点击后播放音频

    private void Start()
    {
        // 确保按钮和AudioSource已设置
        if (playButton != null && audioSource != null)
        {
            playButton.onClick.AddListener(PlayAudio); // 绑定按钮点击事件
        }
        else
        {
            Debug.LogError("Button or AudioSource is not assigned!");
        }
    }

    private void PlayAudio()
    {
        if (audioSource != null)
        {
            audioSource.Play(); // 播放音频
            Debug.Log("Audio is playing.");
        }
    }
}
