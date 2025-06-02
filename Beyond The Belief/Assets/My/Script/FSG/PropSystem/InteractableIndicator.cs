using UnityEngine;

/// <summary>
/// ������ʾָʾ��������һ��UI��ʾͼ�����ʾ�����أ�����"��F����"����ʾ��
/// </summary>
public class InteractableIndicator : MonoBehaviour
{
    [Header("UI����")]
    public GameObject indicatorImage; // ��Unity�༭����������Ҫ��ʾ����ʾUI������һ��Image��Canvas Group��

    // ====== ��ʼ�� ======
    private void Start()
    {
        // ���indicatorImage�Ѹ�ֵ����ʼ״̬��Ϊ����
        if (indicatorImage != null)
        {
            indicatorImage.SetActive(false);
        }
    }

    // ====== ��ʾ��ʾ ======
    public void ShowIndicator()
    {
        // ��ȫ��飺���indicatorImage��Ϊ�գ��򼤻���ʾ
        if (indicatorImage != null)
            indicatorImage.SetActive(true);
    }

    // ====== ������ʾ ======
    public void HideIndicator()
    {
        // ��ȫ��飺���indicatorImage��Ϊ�գ�������
        if (indicatorImage != null)
            indicatorImage.SetActive(false);
    }
}