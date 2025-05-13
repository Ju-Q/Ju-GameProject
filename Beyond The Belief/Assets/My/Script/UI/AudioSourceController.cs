using UnityEngine;
using UnityEngine.UI; // ��Ҫ������������ռ���ʹ��Slider
using System.Collections;

public class AudioSourceController : MonoBehaviour
{
    public AudioSource audioSource; // Ҫ�����AudioSource
    public Slider volumeSlider; // ����������Slider
    public float waitDuration = 5f; // �ȴ�������
    public float startVolume = 0f; // ��ʼ����
    public float targetVolume = 1f; // Ŀ������
    public float changeDuration = 2f; // �����仯�ĳ���ʱ��

    private void Start()
    {
        // ȷ��AudioSource��Slider������
        if (audioSource != null && volumeSlider != null)
        {
            audioSource.volume = startVolume; // ���ó�ʼ����
            volumeSlider.value = startVolume; // ����Slider��ʼֵ
            audioSource.Stop(); // ȷ����Ƶֹͣ
            StartCoroutine(ActivateAudioSourceCoroutine());
        }
        else
        {
            Debug.LogError("AudioSource or Slider is not assigned!");
        }
    }

    private IEnumerator ActivateAudioSourceCoroutine()
    {
        // �ȴ�ָ��������
        yield return new WaitForSeconds(waitDuration);

        // ����AudioSource������
        audioSource.Play();
        Debug.Log("AudioSource activated and started playing.");

        // ��������
        float elapsed = 0f;

        while (elapsed < changeDuration)
        {
            elapsed += Time.deltaTime;
            float currentVolume = Mathf.Lerp(startVolume, targetVolume, elapsed / changeDuration);
            audioSource.volume = currentVolume; // ����AudioSource������
            volumeSlider.value = currentVolume; // ����Slider��ֵ
            yield return null; // �ȴ���һ֡
        }

        audioSource.volume = targetVolume; // ȷ�������ﵽĿ��ֵ
        volumeSlider.value = targetVolume; // ȷ��Slider��ֵҲ����
    }
}
