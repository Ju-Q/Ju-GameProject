using UnityEngine;

public class InteractableIndicator : MonoBehaviour
{
    public GameObject indicatorImage; // ������ʾImage

    private void Start()
    {
        if (indicatorImage != null)
        {
            indicatorImage.SetActive(false); // ��ʼ����
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
