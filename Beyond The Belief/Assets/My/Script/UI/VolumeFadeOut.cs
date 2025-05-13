using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VolumeFadeOut : MonoBehaviour
{
    public AudioSource audioSource; // Ҫ���Ƶ�AudioSource
    public Button button; // �����������͵İ�ť
    public float waitDuration = 2f; // �ȴ�������
    public float fadeDuration = 2f; // ����0��ʱ��

    private void Start()
    {
        // ȷ����ť����¼��Ѱ�
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
        // �ȴ�ָ��������
        yield return new WaitForSeconds(waitDuration);

        float startVolume = audioSource.volume; // ��ȡ��ǰ����
        float elapsed = 0f;

        // ������������
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeDuration);
            yield return null; // �ȴ���һ֡
        }

        audioSource.volume = 0; // ȷ��������ȫ����0
        audioSource.Stop(); // ��ѡ��ֹͣ��Ƶ����
    }
}
