using UnityEngine;

/// <summary>
/// 交互提示指示器：控制一个UI提示图标的显示与隐藏（例如"按F交互"的提示）
/// </summary>
public class InteractableIndicator : MonoBehaviour
{
    [Header("UI配置")]
    public GameObject indicatorImage; // 在Unity编辑器中拖入需要显示的提示UI对象（如一个Image或Canvas Group）

    // ====== 初始化 ======
    private void Start()
    {
        // 如果indicatorImage已赋值，初始状态设为隐藏
        if (indicatorImage != null)
        {
            indicatorImage.SetActive(false);
        }
    }

    // ====== 显示提示 ======
    public void ShowIndicator()
    {
        // 安全检查：如果indicatorImage不为空，则激活显示
        if (indicatorImage != null)
            indicatorImage.SetActive(true);
    }

    // ====== 隐藏提示 ======
    public void HideIndicator()
    {
        // 安全检查：如果indicatorImage不为空，则隐藏
        if (indicatorImage != null)
            indicatorImage.SetActive(false);
    }
}