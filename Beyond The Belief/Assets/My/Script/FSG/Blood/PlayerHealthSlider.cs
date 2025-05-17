using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSlider : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;          // Ѫ��Slider��value��Χ0~1����ʼ1
    public Image bloodImage;             // ����Ѫ���仯͸���ȵ�ͼƬ��alpha 0~255��
    public Image blackoutImage;          // ����Image������ʱalpha�仯

    [Header("Timing Settings")]
    public float decreaseDuration = 5f; // ����TriggerBox��5�����ٽ���0
    public float recoverDuration = 1f;  // �뿪TriggerBox��1�����ٻָ���1

    [Header("Blackout Settings")]
    public float blackoutFadeInTime = 3f;   // ��������ʱ�䣨3�룩
    public float blackoutDuration = 5f;     // ��ȫ����ͣ��ʱ�䣨5�룩
    public float blackoutFadeOutTime = 2f;  // ��������ʱ�䣨2�룩

    [Header("Audio Settings")]
    public AudioSource deathAudioSource;  // ����BGM����������ָ��

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
        // ������������������������healthSlider.value���Ա仯
        if (deathAudioSource != null)
        {
            if (healthSlider.value < 1f && !deathAudioSource.isPlaying)
            {
                deathAudioSource.Play();
            }

            if (deathAudioSource.isPlaying)
            {
                if (isDead) // ����������ֹͣ
                {
                    deathAudioSource.Stop();
                }
                else
                {
                    deathAudioSource.volume = 1f - healthSlider.value; // value��1����0��������0����1
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
        // ��������
        float elapsed = 0f;
        while (elapsed < blackoutFadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / blackoutFadeInTime);
            blackoutImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // ������ȫ������ִ�и���������ָ�Ѫ������������ͼ��
        healthSlider.value = 1f;
        UpdateBloodImageAlpha();

        // ��ɫ���̹�λ
        Transform respawnPoint = RespawnManager.Instance.GetCurrentRespawnPoint();
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        // ��ȫ����
        blackoutImage.color = Color.black;

        // �ȴ�����ͣ��ʱ��
        yield return new WaitForSeconds(blackoutDuration);

        // ��������
        elapsed = 0f;
        while (elapsed < blackoutFadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / blackoutFadeOutTime);
            blackoutImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // ��ɵ������������״̬
        isDead = false;

        // ֹͣ��������
        if (deathAudioSource != null && deathAudioSource.isPlaying)
        {
            deathAudioSource.Stop();
        }
    }
}
