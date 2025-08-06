using StarterAssets;
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

    [Header("Character Components")]
    public Animator playerAnimator;
    public Transform playerModel;

    [Header("Item Pickup Manager")]
    public ItemPickupManager pickupManager;
    public ThirdPersonController Controller;

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

        if (pickupManager == null)
            pickupManager = FindObjectOfType<ItemPickupManager>();
    }

    private void Update()
    {
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
        if (isDead || Controller.isDead) return; // 新增判断

        RespawnPointSetter setter = other.GetComponent<RespawnPointSetter>();
        if (setter != null)
        {
            if (pickupManager != null && pickupManager.propACount >= 3)
            {
                Debug.Log("PropA数量>=3，进入减速区不扣血");
                return;
            }

            isInTrigger = true;

            RespawnManager.Instance.SetCurrentRespawnPoint(setter.respawnPoint);

            StartChangeHealthValue(healthSlider.value, 0f, decreaseDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead || Controller.isDead) return; // 新增判断

        RespawnPointSetter setter = other.GetComponent<RespawnPointSetter>();
        if (setter != null)
        {
            if (pickupManager != null && pickupManager.propACount >= 3)
            {
                Debug.Log("PropA数量>=3，离开减速区不回血");
                return;
            }

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

            // 死亡触发
            if (value <= 0f && !isDead && !Controller.isDead)
            {
                TriggerDeath();
                yield break;
            }

            yield return null;
        }

        healthSlider.value = to;
        UpdateBloodImageAlpha();

        if (healthSlider.value <= 0f && !isDead && !Controller.isDead)
        {
            TriggerDeath();
        }
    }

    private void TriggerDeath()
    {
        isDead = true;
        Controller.isDead = true; // 同步给 ThirdPersonController

        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = true;
            playerAnimator.SetTrigger("MarshDie");
            Controller.canMove = false;
        }

        StartCoroutine(StartBlackout());
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
        // 如果已经由外部触发死亡，这里不重复执行
        if (Controller.isDead && !isDead)
            yield break;

        // 黑屏淡入
        float elapsed = 0f;
        while (elapsed < blackoutFadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / blackoutFadeInTime);
            blackoutImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        blackoutImage.color = Color.black;

        healthSlider.value = 1f;
        UpdateBloodImageAlpha();

        Transform respawnPoint = RespawnManager.Instance.GetCurrentRespawnPoint();
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Rebirth");
        }

        yield return new WaitForSeconds(blackoutDuration);
        playerAnimator.applyRootMotion = false;

        yield return new WaitForSeconds(0.1f);

        if (playerModel != null)
        {
            playerModel.localPosition = Vector3.zero;
            playerModel.localRotation = Quaternion.identity;
        }
        Controller.canMove = true;
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
        Controller.isDead = false;

        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
            playerAnimator.applyRootMotion = false;
        }
    }
}
