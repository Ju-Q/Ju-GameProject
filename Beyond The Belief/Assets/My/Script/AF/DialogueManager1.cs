using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using StarterAssets;

public class DialogueManager1 : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public List<string> dialogues;
    public Image characterPortrait;
    public List<Sprite> characterPortraits;
    public Image backgroundImage;
    public Sprite backgroundSprite;

    [Header("延迟设置")]
    public float startDelay = 0f;

    [Header("音效设置")]
    public AudioSource audioSource; // 播放音效的 AudioSource
    public AudioClip nextDialogueClip; // 开启对话 / 下一句话
    [Range(0f, 1f)] public float nextDialogueVolume = 1f; // 新增：下一句音量

    public AudioClip typingClip; // 打字音效
    [Range(0f, 1f)] public float typingVolume = 1f; // 新增：打字音量

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
            Controller.canMove = false;
            StartCoroutine(StartDialogueWithDelay(startDelay));
        }
        else
        {
            Debug.LogWarning("没有可用的对话内容.");
            isDialogueActive = false;
        }
    }

    IEnumerator StartDialogueWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayNextDialogueSound(); // 延迟结束后播放开启对话音效
        UpdateDialogueUI();
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
            StopTypingEffect(); // 跳过打字
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
        PlayNextDialogueSound(); // 播放下一句音效
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

            // 播放打字音效（不暂停，让声音自然重叠）
            if (typingClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(typingClip, typingVolume); // 使用独立音量
            }

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

        // 直接显示完整对话 → 不播放打字音效
        dialogueText.text = dialogues[currentDialogueIndex];
        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueText.text = "";
        backgroundImage.enabled = false;
        characterPortrait.enabled = false;
        isDialogueActive = false;

        Controller.canMove = true;
        OnDialogueEnd?.Invoke();
    }

    // 播放开启对话 / 下一句的音效
    void PlayNextDialogueSound()
    {
        if (nextDialogueClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(nextDialogueClip, nextDialogueVolume); // 使用独立音量
        }
    }
}
