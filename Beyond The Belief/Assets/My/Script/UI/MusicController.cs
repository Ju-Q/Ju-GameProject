using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource; // ����� AudioSource
    public Slider volumeSlider; // ����� Slider
    public Animator animator; // �� Animator

    private bool isAnimating = false; // �Ƿ�����ͨ��������������

    void Start()
    {
        volumeSlider.value = audioSource.volume; // ��ʼ�� Slider ֵ
        volumeSlider.onValueChanged.AddListener(OnVolumeChange); // ����¼�����
    }

    void Update()
    {
        // �ڶ�������ʱ���� Slider ��������
        audioSource.volume = volumeSlider.value; // ʹ�� Slider ��ֵ
    }

    public void StartAnimation()
    {
        isAnimating = true; // ��ʼ����ʱ����
        animator.SetTrigger("PlayVolumeAnimation"); // ���Ŷ���
        Debug.Log("Animation started");
    }

    public void StopAnimation()
    {
        isAnimating = false; // ��������ʱ����
        Debug.Log("Animation stopped");
    }

    private void OnVolumeChange(float value)
    {
        audioSource.volume = value; // ��������
    }
}
