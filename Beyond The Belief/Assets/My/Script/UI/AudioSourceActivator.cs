using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceActivator : MonoBehaviour
{
    public AudioSource audioSource; // Ҫ�����AudioSource
    public float delayInSeconds = 5f; // �ӳٵ�����

    private void Start()
    {
        // ȷ��AudioSource��δ����״̬
        if (audioSource != null)
        {
            audioSource.Stop();
            //audioSource.volume = 0; // ��ѡ����������Ϊ0�Ա�����������
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
        // �ȴ�ָ��������
        yield return new WaitForSeconds(delayInSeconds);

        // ����AudioSource
        if (audioSource != null)
        {
            audioSource.volume = 1; // �ָ�����
            audioSource.Play(); // ������Ƶ
            Debug.Log("AudioSource activated and playing.");
        }
        else
        {
            Debug.LogError("AudioSource is null after delay.");
        }
    }
}
