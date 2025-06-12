using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class AudioFadeInTrigger : MonoBehaviour
{
    [Header("主音频设置")]
    public AudioSource mainAudioSource;
    public float targetVolume = 1f;
    public float fadeInDuration = 2f;

    [Header("（可选）需要淡出的其他音频")]
    public AudioSource[] otherAudioSources;
    public float fadeOutDuration = 2f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (mainAudioSource != null)
        {
            mainAudioSource.volume = 0f;
            mainAudioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            // 开始主音频淡入
            if (mainAudioSource != null)
            {
                mainAudioSource.Play();
                StartCoroutine(FadeInAudio(mainAudioSource, targetVolume, fadeInDuration));
            }

            // 同步淡出其他音频
            if (otherAudioSources != null && otherAudioSources.Length > 0)
            {
                foreach (AudioSource source in otherAudioSources)
                {
                    if (source != null && source.isPlaying)
                    {
                        StartCoroutine(FadeOutAudio(source, fadeOutDuration));
                    }
                }
            }
        }
    }

    private IEnumerator FadeInAudio(AudioSource source, float targetVol, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVol, timer / duration);
            yield return null;
        }
        source.volume = targetVol;
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }
}
