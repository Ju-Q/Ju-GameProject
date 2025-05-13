using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider; // 引用到 Slider 组件
    public float stepAmount = 0.1f; // 每次按键变化的幅度

    void Update()
    {
        // 检测按左键
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // 减少 Slider 值
            slider.value = Mathf.Clamp(slider.value - stepAmount, slider.minValue, slider.maxValue);
        }

        // 检测按右键
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 增加 Slider 值
            slider.value = Mathf.Clamp(slider.value + stepAmount, slider.minValue, slider.maxValue);
        }
    }
}