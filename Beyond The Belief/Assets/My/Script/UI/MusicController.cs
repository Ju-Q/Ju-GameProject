using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    public AudioSource audioSource; // 绑定你的 AudioSource
    public Slider volumeSlider; // 绑定你的 Slider
    public Animator animator; // 绑定 Animator

    private bool isAnimating = false; // 是否正在通过动画控制音量

    void Start()
    {
        volumeSlider.value = audioSource.volume; // 初始化 Slider 值
        volumeSlider.onValueChanged.AddListener(OnVolumeChange); // 添加事件监听
    }

    void Update()
    {
        // 在动画进行时允许 Slider 控制音量
        audioSource.volume = volumeSlider.value; // 使用 Slider 的值
    }

    public void StartAnimation()
    {
        isAnimating = true; // 开始动画时设置
        animator.SetTrigger("PlayVolumeAnimation"); // 播放动画
        Debug.Log("Animation started");
    }

    public void StopAnimation()
    {
        isAnimating = false; // 动画结束时设置
        Debug.Log("Animation stopped");
    }

    private void OnVolumeChange(float value)
    {
        audioSource.volume = value; // 更新音量
    }
}
