using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RotatingObject
{
    public Transform targetObject;
    public Vector3 targetEulerAngles;
    public float rotationDuration = 1f;
}

[System.Serializable]
public class AudioFadeIn
{
    public AudioSource audioSource;
    public AudioClip clip;
    public float targetVolume = 1f;
    public float fadeInDuration = 1f;
}

public class SlowDownAnimator : MonoBehaviour
{
    [Header("动画减速设置")]
    public Animator targetAnimator;
    public string targetStateName;
    public float slowDownDuration = 2f;

    [Header("附加旋转对象")]
    public List<RotatingObject> rotatingObjects = new List<RotatingObject>();

    [Header("多音频设置")]
    public List<AudioFadeIn> audioClips = new List<AudioFadeIn>();

    private bool isPlayerInZone = false;
    private bool isSlowingDown = false;
    private float originalSpeed = 1f;

    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.F) && !isSlowingDown)
        {
            // 播放所有音频并渐变音量
            foreach (var audioData in audioClips)
            {
                if (audioData.audioSource != null)
                {
                    audioData.audioSource.clip = audioData.clip;
                    audioData.audioSource.volume = 0f;
                    audioData.audioSource.Play();
                    StartCoroutine(FadeInAudio(audioData));
                }
            }

            // 开始动画减速
            StartCoroutine(SlowDownAnimation());

            // 同步旋转所有物体
            foreach (var rotObj in rotatingObjects)
            {
                StartCoroutine(RotateObject(rotObj));
            }
        }
    }

    private IEnumerator SlowDownAnimation()
    {
        isSlowingDown = true;
        float timer = 0f;
        originalSpeed = targetAnimator.speed;

        while (timer < slowDownDuration)
        {
            timer += Time.deltaTime;
            targetAnimator.speed = Mathf.Lerp(originalSpeed, 0f, timer / slowDownDuration);
            yield return null;
        }

        targetAnimator.speed = 0f;
        isSlowingDown = false;
    }

    private IEnumerator RotateObject(RotatingObject rotObj)
    {
        if (rotObj.targetObject == null) yield break;

        Quaternion initialRotation = rotObj.targetObject.localRotation;
        Quaternion targetRotation = Quaternion.Euler(rotObj.targetEulerAngles);

        float timer = 0f;
        while (timer < rotObj.rotationDuration)
        {
            timer += Time.deltaTime;
            rotObj.targetObject.localRotation = Quaternion.Slerp(initialRotation, targetRotation, timer / rotObj.rotationDuration);
            yield return null;
        }

        rotObj.targetObject.localRotation = targetRotation;
    }

    private IEnumerator FadeInAudio(AudioFadeIn audioData)
    {
        float timer = 0f;
        while (timer < audioData.fadeInDuration)
        {
            timer += Time.deltaTime;
            audioData.audioSource.volume = Mathf.Lerp(0f, audioData.targetVolume, timer / audioData.fadeInDuration);
            yield return null;
        }
        audioData.audioSource.volume = audioData.targetVolume;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }
}
