using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueChoiceTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string playerTag = "Player";
    private bool isPlayerInside = false;

    [Header("UI Settings")]
    public GameObject choicePanel;
    public Image[] optionImages;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color disabledColor = Color.gray;

    [Header("Grid Settings")]
    public int columns = 2; // 列数

    [Header("Dialogue Settings")]
    public GameObject[] dialogueCanvases;
    private bool[] optionUsed;

    [Header("Prompt UI Settings")]
    public CanvasGroup promptUI;   // 提示UI（World Space Canvas）
    public float fadeDuration = 0.3f; // 淡入淡出时间

    private int currentIndex = 0;
    private bool isChoosing = false;

    private void Start()
    {
        optionUsed = new bool[optionImages.Length];
        UpdateOptionHighlight();
        choicePanel.SetActive(false);

        if (promptUI != null)
        {
            promptUI.alpha = 0;
            promptUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 按F打开选项
        if (isPlayerInside && !isChoosing && Input.GetKeyDown(KeyCode.F))
        {
            isChoosing = true;
            choicePanel.SetActive(true);
            UpdateOptionHighlight();

            // 隐藏提示UI
            HidePrompt();
        }

        if (isChoosing)
        {
            HandleNavigation();

            // 选择
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!optionUsed[currentIndex])
                {
                    ActivateDialogue(currentIndex);
                }
            }

            // 退出
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isChoosing = false;
                choicePanel.SetActive(false);
                ShowPrompt();
            }
        }
    }

    private void HandleNavigation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int newIndex = currentIndex - columns;
            if (newIndex >= 0) currentIndex = newIndex;
            UpdateOptionHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int newIndex = currentIndex + columns;
            if (newIndex < optionImages.Length) currentIndex = newIndex;
            UpdateOptionHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentIndex % columns != 0) currentIndex--;
            UpdateOptionHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if ((currentIndex + 1) % columns != 0 && currentIndex + 1 < optionImages.Length) currentIndex++;
            UpdateOptionHighlight();
        }
    }

    private void UpdateOptionHighlight()
    {
        for (int i = 0; i < optionImages.Length; i++)
        {
            if (optionUsed[i])
            {
                optionImages[i].color = disabledColor;
            }
            else if (i == currentIndex)
            {
                optionImages[i].color = highlightColor;
            }
            else
            {
                optionImages[i].color = normalColor;
            }
        }
    }

    private void ActivateDialogue(int index)
    {
        if (index >= 0 && index < dialogueCanvases.Length && dialogueCanvases[index] != null)
        {
            dialogueCanvases[index].SetActive(true);

            DialogueManager1 dm = dialogueCanvases[index].GetComponent<DialogueManager1>();
            if (dm != null)
            {
                dm.OnDialogueEnd += () =>
                {
                    optionUsed[index] = true;
                    UpdateOptionHighlight();
                    ShowPrompt();
                };
            }

            choicePanel.SetActive(false);
            isChoosing = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            HidePrompt();
        }
    }

    private void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(promptUI, promptUI.alpha, 1f));
        }
    }

    private void HidePrompt()
    {
        if (promptUI != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndDisable(promptUI));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator FadeOutAndDisable(CanvasGroup cg)
    {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f);
        cg.gameObject.SetActive(false);
    }
}
