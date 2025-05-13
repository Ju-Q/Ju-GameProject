using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider; // ���õ� Slider ���
    public float stepAmount = 0.1f; // ÿ�ΰ����仯�ķ���

    void Update()
    {
        // ��ⰴ���
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // ���� Slider ֵ
            slider.value = Mathf.Clamp(slider.value - stepAmount, slider.minValue, slider.maxValue);
        }

        // ��ⰴ�Ҽ�
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // ���� Slider ֵ
            slider.value = Mathf.Clamp(slider.value + stepAmount, slider.minValue, slider.maxValue);
        }
    }
}