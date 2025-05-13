using System.Collections;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject canvasToActivate; // Ҫ����� Canvas
    public Animator animator; // ���ڶ������Ƶ� Animator
    public string openTrigger; // ��ʱ�Ĵ���������
    public string closeTrigger; // �ر�ʱ�Ĵ���������
    public AudioSource audioSource; // ��ƵԴ
    public AudioClip openSound; // ��ʱ���ŵ���Ƶ
    public AudioClip closeSound; // �ر�ʱ���ŵ���Ƶ

    private bool isCanvasActive = false; // Canvas ��ǰ״̬
    private int sortingOrder = 100; // ȷ�� Canvas �����ϲ������ֵ

    void Update()
    {
        // ��� F1 ���Ƿ񱻰���
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleCanvas();
        }
    }

    void ToggleCanvas()
    {
        // �л� Canvas �ļ���״̬
        isCanvasActive = !isCanvasActive;

        if (isCanvasActive)
        {
            // ���� Canvas
            canvasToActivate.SetActive(true);
            // ȷ�� Canvas ����ǰ��
            Canvas canvas = canvasToActivate.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder; // ��������㼶
            }
            animator.SetTrigger(openTrigger); // ���Ŵ򿪶���

            // ���Ŵ���Ƶ
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // ��ʾ���
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // �������
        }
        else
        {
            // �ر� Canvas �������رն���
            animator.SetTrigger(closeTrigger); // ���Źرն���

            // ���Źر���Ƶ
            if (audioSource != null && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }

            StartCoroutine(DeactivateCanvasAfterAnimation());
        }
    }

    private IEnumerator DeactivateCanvasAfterAnimation()
    {
        // �ȴ� Animator ������رն���
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        canvasToActivate.SetActive(false); // �ر� Canvas

        // ���ع��
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // �������
    }
}
