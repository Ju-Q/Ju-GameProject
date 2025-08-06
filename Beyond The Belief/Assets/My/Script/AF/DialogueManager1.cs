using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System; // 添加以支持 Action
using StarterAssets;

public class DialogueManager1 : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public List<string> dialogues;
    public Image characterPortrait;
    public List<Sprite> characterPortraits;
    public Image backgroundImage;
    public Sprite backgroundSprite;

    private int currentDialogueIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isDialogueActive = true;

    private float inputCooldown = 0.3f;
    private bool inputLocked = false;
    private ThirdPersonController Controller;

    // 对话结束事件
    public Action OnDialogueEnd;

    void Start()
    {
        Controller = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonController>();
        if (dialogues.Count > 0)
        {
            backgroundImage.enabled = false;
            characterPortrait.enabled = false;
            UpdateDialogueUI();
            Controller.canMove = false;
        }
        else
        {
            Debug.LogWarning("没有可用的对话内容.");
            isDialogueActive = false;
        }
    }

    void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Return) && !inputLocked)
        {
            inputLocked = true;
            ShowNextDialogue();
            StartCoroutine(UnlockInputAfterDelay(inputCooldown));
        }
    }

    IEnumerator UnlockInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputLocked = false;
    }

    void ShowNextDialogue()
    {
        if (!isDialogueActive) return;

        if (isTyping)
        {
            StopTypingEffect();
            return;
        }

        if (currentDialogueIndex < dialogues.Count - 1)
        {
            StartCoroutine(DelayNextDialogue(0.5f));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator DelayNextDialogue(float delay)
    {
        isDialogueActive = false;
        yield return new WaitForSeconds(delay);
        currentDialogueIndex++;
        isDialogueActive = true;
        UpdateDialogueUI();
    }

    void UpdateDialogueUI()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeDialogue(dialogues[currentDialogueIndex]));

        if (currentDialogueIndex < characterPortraits.Count && characterPortraits[currentDialogueIndex] != null)
        {
            characterPortrait.sprite = characterPortraits[currentDialogueIndex];
            characterPortrait.enabled = true;
        }
        else
        {
            characterPortrait.enabled = false;
        }

        if (backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.enabled = true;
        }
        else
        {
            backgroundImage.enabled = false;
        }
    }

    IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }

    void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = dialogues[currentDialogueIndex];
        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueText.text = "";
        backgroundImage.enabled = false;
        characterPortrait.enabled = false;
        isDialogueActive = false;

        //Debug.Log("对话序列结束.");
        Controller.canMove = true;
        // 触发事件通知外部
        OnDialogueEnd?.Invoke();
    }
}
