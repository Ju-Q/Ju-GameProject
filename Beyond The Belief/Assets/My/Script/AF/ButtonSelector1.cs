using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ButtonDialogue
{
    public List<string> content;
    public List<Texture> speakerImages;
    public Texture backgroundImage;
}

public class ButtonSelector1 : MonoBehaviour
{
    public GameObject canvas;
    public CanvasGroup dialogueCanvasGroup; // 对话UI CanvasGroup

    // 按钮、对话文本、说话者图片
    public List<Button> buttons;
    public List<TMP_Text> dialogueTexts;
    public List<RawImage> speakerImages;

    public List<ButtonDialogue> dialogues;
    private int selectedButton = 0;
    private int currentDialogueIndex = 0;
    private bool isInTrigger = false;
    private bool isDisplayingDialogue = false;

    // 按键绑定
    public KeyCode toggleKey = KeyCode.G;
    public KeyCode selectLeftKey = KeyCode.A;
    public KeyCode selectRightKey = KeyCode.D;
    public KeyCode confirmKey = KeyCode.Return;
    public KeyCode nextDialogueKey = KeyCode.Space;

    // 选中按钮颜色
    public Color selectedButtonColor = Color.yellow;

    // 打字机速度
    public float typingSpeed = 0.05f;

    // 背景图
    public RawImage backgroundImage;

    // ==== 新增交互提示UI ====
    public CanvasGroup interactionPrompt;
    public float fadeDuration = 0.3f;
    private Coroutine promptFadeCoroutine;
    private Coroutine dialogueFadeCoroutine;
    // =======================
    private bool isTyping = false;       // 是否正在打字
    private bool lineFinished = false;   // 当前句子是否完全显示
    private bool skipToFullLine = false; // 玩家是否要求跳到全文


    void Start()
    {
        canvas.SetActive(false);

        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.sortingOrder = 10;
        }

        HideAllSpeakerImages();
        UpdateButtonSelection();

        if (interactionPrompt != null)
        {
            interactionPrompt.alpha = 0f;
            interactionPrompt.gameObject.SetActive(false);
        }

        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0f;
            dialogueCanvasGroup.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isInTrigger)
        {
            if (Input.GetKeyDown(toggleKey))
            {
                HideInteractionPrompt(); // 按交互键先隐藏提示

                if (!canvas.activeSelf)
                {
                    ShowDialogueUI();
                }
                else
                {
                    HideDialogueUI();
                }
            }

            if (canvas.activeSelf)
            {
                if (Input.GetKeyDown(selectLeftKey))
                {
                    selectedButton = (selectedButton - 1 + dialogues.Count) % dialogues.Count;
                    currentDialogueIndex = 0;
                    UpdateButtonSelection();
                }
                else if (Input.GetKeyDown(selectRightKey))
                {
                    selectedButton = (selectedButton + 1) % dialogues.Count;
                    currentDialogueIndex = 0;
                    UpdateButtonSelection();
                }

                if (Input.GetKeyDown(confirmKey) && !isDisplayingDialogue)
                {
                    isDisplayingDialogue = true;
                    StartCoroutine(DisplayDialogueWithTypingEffect(dialogues[selectedButton]));
                    HideAllButtons();
                }
            }
        }
    }

    private void UpdateButtonSelection()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            ColorBlock colors = buttons[i].colors;
            colors.normalColor = (i == selectedButton) ? selectedButtonColor : Color.white;
            buttons[i].colors = colors;
        }
    }

    private IEnumerator DisplayDialogueWithTypingEffect(ButtonDialogue dialogue)
    {
        TMP_Text currentDialogueText = null;
        RawImage currentSpeakerImage = null;

        if (backgroundImage != null)
        {
            backgroundImage.texture = dialogue.backgroundImage;
            backgroundImage.gameObject.SetActive(true);
        }

        if (selectedButton >= 0 && selectedButton < dialogueTexts.Count)
        {
            currentDialogueText = dialogueTexts[selectedButton];
            currentSpeakerImage = speakerImages[selectedButton];
        }

        currentDialogueIndex = 0;

        if (currentSpeakerImage != null && currentDialogueIndex < dialogue.speakerImages.Count)
        {
            currentSpeakerImage.texture = dialogue.speakerImages[currentDialogueIndex];
            currentSpeakerImage.gameObject.SetActive(true);
        }

        while (currentDialogueIndex < dialogue.content.Count)
        {
            // 打印当前句子
            yield return StartCoroutine(TypeText(currentDialogueText, dialogue.content[currentDialogueIndex]));

            // 等待玩家按下下一句键（只有在句子显示完整后才有效）
            bool nextPressed = false;
            while (!nextPressed)
            {
                if (Input.GetKeyDown(nextDialogueKey) && lineFinished)
                {
                    nextPressed = true;
                }
                yield return null;
            }

            currentDialogueIndex++;

            if (currentDialogueIndex >= dialogue.content.Count)
            {
                currentDialogueText.text = "";
                if (currentSpeakerImage != null)
                    currentSpeakerImage.gameObject.SetActive(false);

                if (backgroundImage != null)
                {
                    backgroundImage.texture = null;
                    backgroundImage.gameObject.SetActive(false);
                }

                isDisplayingDialogue = false;
                HideAllButtons();
                HideDialogueUI();
                ShowInteractionPrompt();
                yield break;
            }
            else
            {
                if (currentSpeakerImage != null && currentDialogueIndex < dialogue.speakerImages.Count)
                {
                    currentSpeakerImage.texture = dialogue.speakerImages[currentDialogueIndex];
                    currentSpeakerImage.gameObject.SetActive(true);
                }
            }
        }
    }

    private IEnumerator TypeText(TMP_Text dialogueText, string line)
    {
        isTyping = true;
        lineFinished = false;
        skipToFullLine = false;
        dialogueText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            if (Input.GetKeyDown(nextDialogueKey))
            {
                skipToFullLine = true;
            }

            if (skipToFullLine)
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += line[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        lineFinished = true; // 标记当前句子已经完全显示
    }

    private void HideAllButtons()
    {
        foreach (var button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
    }

    private void ShowAllButtons()
    {
        foreach (var button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(true);
        }
    }

    private void HideAllSpeakerImages()
    {
        foreach (var speakerImage in speakerImages)
        {
            if (speakerImage != null)
                speakerImage.gameObject.SetActive(false);
        }
    }

    // ==== 交互提示淡入淡出 ====
    private void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            if (promptFadeCoroutine != null) StopCoroutine(promptFadeCoroutine);
            promptFadeCoroutine = StartCoroutine(FadeCanvasGroup(interactionPrompt, 0f, 1f, fadeDuration));
        }
    }

    private void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            if (promptFadeCoroutine != null) StopCoroutine(promptFadeCoroutine);
            promptFadeCoroutine = StartCoroutine(FadeCanvasGroup(interactionPrompt, 1f, 0f, fadeDuration));
        }
    }
    // ========================

    // ==== 对话UI淡入淡出 ====
    private void ShowDialogueUI()
    {
        if (dialogueFadeCoroutine != null) StopCoroutine(dialogueFadeCoroutine);
        canvas.SetActive(true);
        dialogueFadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, fadeDuration));
        currentDialogueIndex = 0;
        UpdateButtonSelection();
        ShowAllButtons();
    }

    private void HideDialogueUI()
    {
        if (dialogueFadeCoroutine != null) StopCoroutine(dialogueFadeCoroutine);
        dialogueFadeCoroutine = StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, fadeDuration, () =>
        {
            canvas.SetActive(false);
        }));
    }
    // =======================

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration, System.Action onComplete = null)
    {
        cg.gameObject.SetActive(true);
        cg.alpha = start;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
        if (end == 0f) cg.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = true;
            canvas.SetActive(false);
            ShowInteractionPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = false;
            HideInteractionPrompt();
            HideDialogueUI();
        }
    }
}
