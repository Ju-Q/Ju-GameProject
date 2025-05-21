using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSlider : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public Image bloodImage;
    public Image blackoutImage;

    [Header("Timing Settings")]
    public float decreaseDuration = 5f;
    public float recoverDuration = 1f;

    [Header("Blackout Settings")]
    public float blackoutFadeInTime = 3f;
    public float blackoutDuration = 5f;
    public float blackoutFadeOutTime = 2f;

    [Header("Audio Settings")]
    public AudioSource deathAudioSource;

    private bool isInTrigger = false;
    private bool isDead = false;
    private Coroutine changeHealthCoroutine;

    [Header("Character Components")]
    public Animator playerAnimator;             // 角色Animator
    public Transform playerModel;               // 模型Transform（角色模型作为子对象）

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
        // 控制死亡音乐音量
        if (deathAudioSource != null)
        {
            if (healthSlider.value < 1f && !deathAudioSource.isPlaying)
            {
                deathAudioSource.Play();
            }

            if (deathAudioSource.isPlaying)
            {
                if (isDead)
                {
                    deathAudioSource.Stop();
                }
                else
                {
                    deathAudioSource.volume = 1f - healthSlider.value;
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

                // 播放死亡动画
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("MarshDie");
                }

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

            // 播放死亡动画
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("MarshDie");
            }

            StartCoroutine(StartBlackout());
        }
    }

    private void UpdateBloodImageAlpha()
    {
        if (bloodImage != null)
        {
            float alpha = 1f - healthSlider.value;
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

        // 黑屏完全
        blackoutImage.color = Color.black;

        // 设置血量、隐藏受伤UI
        healthSlider.value = 1f;
        UpdateBloodImageAlpha();

        // 重置玩家到重生点
        Transform respawnPoint = RespawnManager.Instance.GetCurrentRespawnPoint();
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        // 播放 Rebirth 动画
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Rebirth");
        }

     

        // 黑屏停留
        yield return new WaitForSeconds(blackoutDuration);

        // 等待动画开始（或插入一点延迟），再归位模型
        yield return new WaitForSeconds(0.1f); // 可根据动画设置改成 0.2f~0.3f

        if (playerModel != null)
        {
            playerModel.localPosition = Vector3.zero;
            playerModel.localRotation = Quaternion.identity;
        }

        // 黑屏淡出
        elapsed = 0f;
        while (elapsed < blackoutFadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / blackoutFadeOutTime);
            blackoutImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 清除死亡状态
        isDead = false;

        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
        }
    }

}
