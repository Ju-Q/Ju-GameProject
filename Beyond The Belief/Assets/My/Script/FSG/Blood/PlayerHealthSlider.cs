using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSlider : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;          // 血条Slider，value范围0~1，初始1
    public Image bloodImage;             // 跟随血条变化透明度的图片（alpha 0~255）
    public Image blackoutImage;          // 黑屏Image，黑屏时alpha变化

    [Header("Timing Settings")]
    public float decreaseDuration = 5f; // 进入TriggerBox后5秒匀速降到0
    public float recoverDuration = 1f;  // 离开TriggerBox后1秒匀速恢复到1

    [Header("Blackout Settings")]
    public float blackoutFadeInTime = 3f;   // 黑屏淡入时间（3秒）
    public float blackoutDuration = 5f;     // 完全黑屏停留时间（5秒）
    public float blackoutFadeOutTime = 2f;  // 黑屏淡出时间（2秒）

    [Header("Audio Settings")]
    public AudioSource deathAudioSource;  // 死亡BGM播放器，需指定

    private bool isInTrigger = false;
    private bool isDead = false;

    private Coroutine changeHealthCoroutine;

    private void Start()
    {
        if (healthSlider != null)
            healthSlider.value = 1f;

        if (blackoutImage != null)
            blackoutImage.color = new Color(0, 0, 0, 0);

        UpdateBloodImageAlpha();
    }

    private void Update()
    {
        // 控制死亡音乐音量，音量随healthSlider.value线性变化
        if (deathAudioSource != null)
        {
            if (healthSlider.value < 1f && !deathAudioSource.isPlaying)
            {
                deathAudioSource.Play();
            }

            if (deathAudioSource.isPlaying)
            {
                if (isDead) // 死亡后声音停止
                {
                    deathAudioSource.Stop();
                }
                else
                {
                    deathAudioSource.volume = 1f - healthSlider.value; // value从1降到0，音量从0升到1
                    if (healthSlider.value >= 1f)
                        deathAudioSource.Stop();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        RespawnPointSetter setter = other.GetComponent<RespawnPointSetter>();
        if (setter != null)
        {
            isInTrigger = true;

            RespawnManager.Instance.SetCurrentRespawnPoint(setter.respawnPoint);

            StartChangeHealthValue(healthSlider.value, 0f, decreaseDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead) return;

        RespawnPointSetter setter = other.GetComponent<RespawnPointSetter>();
        if (setter != null)
        {
            isInTrigger = false;
            StartChangeHealthValue(healthSlider.value, 1f, recoverDuration);
        }
    }

    private void StartChangeHealthValue(float from, float to, float duration)
    {
        if (changeHealthCoroutine != null)
        {
            StopCoroutine(changeHealthCoroutine);
        }
        changeHealthCoroutine = StartCoroutine(ChangeHealthValueCoroutine(from, to, duration));
    }

    private IEnumerator ChangeHealthValueCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(from, to, elapsed / duration);
            healthSlider.value = value;
            UpdateBloodImageAlpha();

            if (value <= 0f && !isDead)
            {
                isDead = true;
                StartCoroutine(StartBlackout());
                yield break;
            }

            yield return null;
        }

        healthSlider.value = to;
        UpdateBloodImageAlpha();

        if (healthSlider.value <= 0f && !isDead)
        {
            isDead = true;
            StartCoroutine(StartBlackout());
        }
    }

    private void UpdateBloodImageAlpha()
    {
        if (bloodImage != null)
        {
            float alpha = 1f - healthSlider.value; // value=1 alpha=0; value=0 alpha=1
            Color c = bloodImage.color;
            c.a = alpha;
            bloodImage.color = c;
        }
    }

    private IEnumerator StartBlackout()
    {
        // 黑屏淡入
        float elapsed = 0f;
        while (elapsed < blackoutFadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / blackoutFadeInTime);
            blackoutImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 黑屏完全后立即执行复活操作（恢复血量和隐藏受伤图）
        healthSlider.value = 1f;
        UpdateBloodImageAlpha();

        // 角色立刻归位
        Transform respawnPoint = RespawnManager.Instance.GetCurrentRespawnPoint();
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        // 完全黑屏
        blackoutImage.color = Color.black;

        // 等待黑屏停留时间
        yield return new WaitForSeconds(blackoutDuration);

        // 黑屏淡出
        elapsed = 0f;
        while (elapsed < blackoutFadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / blackoutFadeOutTime);
            blackoutImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 完成淡出，解除死亡状态
        isDead = false;

        // 停止死亡音乐
        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
        }
    }
}
