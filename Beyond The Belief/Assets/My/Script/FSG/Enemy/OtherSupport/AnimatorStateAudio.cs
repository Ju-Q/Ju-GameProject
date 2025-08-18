using UnityEngine;

[System.Serializable]
public class AnimatorAudioPair
{
    public string stateName;
    public AudioClip audioClip;
    public float fadeOutTime = 0.5f;
}

public class AnimatorStateAudio : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator targetAnimator;
    public AnimatorAudioPair[] stateAudioPairs;

    [Header("Audio Settings")]
    public AudioSource audioSource;

    private AnimatorAudioPair currentClipPair = null;
    private float targetVolume = 0f;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = 0f;
        audioSource.loop = true;
    }

    void Update()
    {
        if (targetAnimator == null || stateAudioPairs.Length == 0) return;

        AnimatorStateInfo animatorState = targetAnimator.GetCurrentAnimatorStateInfo(0);

        AnimatorAudioPair newClipPair = null;

        // 检查 Animator 当前状态是否在列表中
        foreach (var pair in stateAudioPairs)
        {
            if (animatorState.IsName(pair.stateName))
            {
                newClipPair = pair;
                break;
            }
        }

        // 如果进入新的状态并且有音效
        if (newClipPair != null && newClipPair != currentClipPair)
        {
            currentClipPair = newClipPair;
            audioSource.clip = currentClipPair.audioClip;
            audioSource.Play();
            targetVolume = 1f;
        }
        // 如果离开状态，需要淡出
        else if (newClipPair == null && currentClipPair != null)
        {
            targetVolume = 0f;
        }

        // 平滑淡入淡出
        if (audioSource.clip != null)
        {
            float fadeSpeed = (currentClipPair != null && currentClipPair.fadeOutTime > 0) ? 1f / currentClipPair.fadeOutTime : 10f;
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

            if (audioSource.volume <= 0.01f && targetVolume == 0f)
            {
                audioSource.Stop();
                audioSource.clip = null;
                currentClipPair = null;
            }
        }
    }
}
