using System.Collections;
using UnityEngine;

public class DialogueManager2 : MonoBehaviour
{
    public GameObject ShowCanvas; // ������ʾ�Ի��� Canvas
    private bool isCanvasActive = false;
    private bool isWaitingForInput = false;
    private bool hasDialogueBeenShown = false; // ��¼�Ի��Ƿ��Ѿ���ʾ��
    public float showCanvasTime = 5f;

    private void Start()
    {
        ShowCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasDialogueBeenShown)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isCanvasActive)
        {
            StopAllCoroutines(); // ȷ�����뿪����ʱֹͣ�κν����еĶԻ���ʾ
            StartCoroutine(FadeOutCanvas());
        }
    }

    private IEnumerator ShowDialogue()
    {
        hasDialogueBeenShown = true; // ��ǶԻ��Ѿ���ʾ��
        isCanvasActive = true;
        ShowCanvas.SetActive(true);

        // ���õ���Ч��
        yield return StartCoroutine(FadeInCanvas());

        isWaitingForInput = false;

        // ������ʾ�Ի����߼�������ֻ��ʾ��ʾ���ֻ����� UI Ԫ�أ�
        yield return new WaitForSeconds(showCanvasTime); // ����Ի�ͣ�� 5 ��

        isWaitingForInput = true;

        while (isWaitingForInput)
        {
            if (Input.GetKeyDown(KeyCode.Return)) // ���س���
            {
                StartCoroutine(FadeOutCanvas());
                yield break;
            }
            yield return null; // �ȴ���һ֡
        }
    }

    private IEnumerator FadeInCanvas()
    {
        float fadeDuration = 0.1f; // ����ʱ��
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>(); // ���û�� CanvasGroup ��������һ��
            canvasGroup.alpha = 0f; // ��ʼ��Ϊ��ȫ͸��
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f; // ȷ������͸��������Ϊ��ȫ��͸��
    }

    private IEnumerator FadeOutCanvas()
    {
        float fadeDuration = 0.1f; // ����ʱ��
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>(); // ���û�� CanvasGroup ��������һ��
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        ShowCanvas.SetActive(false);
        isCanvasActive = false;
    }
}
