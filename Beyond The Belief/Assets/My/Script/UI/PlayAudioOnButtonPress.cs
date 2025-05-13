using UnityEngine;
using UnityEngine.UI; // ����UI�����ռ���ʹ��Button

public class PlayAudioOnButtonPress : MonoBehaviour
{
    public AudioSource audioSource; // Ҫ���ŵ�AudioSource
    public Button playButton; // ��ť������󲥷���Ƶ

    private void Start()
    {
        // ȷ����ť��AudioSource������
        if (playButton != null && audioSource != null)
        {
            playButton.onClick.AddListener(PlayAudio); // �󶨰�ť����¼�
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
            audioSource.Play(); // ������Ƶ
            Debug.Log("Audio is playing.");
        }
    }
}
