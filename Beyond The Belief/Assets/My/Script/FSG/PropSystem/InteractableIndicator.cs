using UnityEngine;

public class InteractableIndicator : MonoBehaviour
{
    public GameObject indicatorImage; // 拖入提示Image

    private void Start()
    {
        if (indicatorImage != null)
        {
            indicatorImage.SetActive(false); // 初始隐藏
        }
    }

    public void ShowIndicator()
    {
        if (indicatorImage != null)
            indicatorImage.SetActive(true);
    }

    public void HideIndicator()
    {
        if (indicatorImage != null)
            indicatorImage.SetActive(false);
    }
}
