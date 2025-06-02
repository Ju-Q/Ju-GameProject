using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager1 : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // 对话文本显示
    public List<string> dialogues; // 对话内容列表
    public Image characterPortrait; // 角色头像
    public List<Sprite> characterPortraits; // 角色头像列表
    public Image backgroundImage; // 背景图片
    public Sprite backgroundSprite; // 背景精灵

    private int currentDialogueIndex = 0; // 当前对话索引
    private Coroutine typingCoroutine; // 打字效果协程
    private bool isTyping = false; // 是否正在打字
    private bool isDialogueActive = true; // 对话是否激活

    private float inputCooldown = 0.3f; // 输入冷却时间
    private float lastInputTime = -1f; // 上次输入时间
    private bool inputLocked = false; // 输入锁定标志

    void Start()
    {
        if (dialogues.Count > 0)
        {
            // 初始隐藏背景和头像
            backgroundImage.enabled = false;
            characterPortrait.enabled = false;
            UpdateDialogueUI();
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

        // 检测回车键按下
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 检查输入是否可用
            if (!inputLocked && Time.time - lastInputTime > inputCooldown)
            {
                inputLocked = true;  // 锁定输入
                lastInputTime = Time.time;
                ShowNextDialogue();
                StartCoroutine(UnlockInputAfterDelay(inputCooldown)); // 延迟解锁
            }
        }
    }

    // 延迟解锁输入
    IEnumerator UnlockInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        inputLocked = false;
    }

    // 显示下一条对话
      void ShowNextDialogue()
    {
        if (!isDialogueActive) return;

        // 如果正在打字，则直接完成当前打字
        if (isTyping)
        {
            StopTypingEffect();
            return;
        }

        // 检查是否还有更多对话
        if (currentDialogueIndex < dialogues.Count - 1)
        {
            // 启动协程延迟切换到下一条对话
            StartCoroutine(DelayNextDialogue(0.5f)); // 添加0.5秒延迟
        }
        else
        {
            EndDialogue();
        }
    }

    // 延迟切换到下一条对话
    IEnumerator DelayNextDialogue(float delay)
    {
        // 在延迟期间禁用输入
        isDialogueActive = false;
        yield return new WaitForSeconds(delay);
        
        // 延迟结束后切换到下一条对话
        currentDialogueIndex++;
        isDialogueActive = true;
        UpdateDialogueUI();
    }

    // 更新对话UI
    void UpdateDialogueUI()
    {
        // 停止之前的打字效果
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 开始新的打字效果
        typingCoroutine = StartCoroutine(TypeDialogue(dialogues[currentDialogueIndex]));

        // 更新角色头像
        if (currentDialogueIndex < characterPortraits.Count && characterPortraits[currentDialogueIndex] != null)
        {
            characterPortrait.sprite = characterPortraits[currentDialogueIndex];
            characterPortrait.enabled = true;
        }
        else
        {
            characterPortrait.enabled = false;
        }

        // 更新背景
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

    // 打字效果协程
    IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        // 逐个字符显示
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // 每个字符间隔0.05秒
        }

        isTyping = false;
    }

    // 停止打字效果，直接显示完整文本
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

    // 结束对话
    void EndDialogue()
    {
        dialogueText.text = "";
        backgroundImage.enabled = false;
        characterPortrait.enabled = false;
        isDialogueActive = false;

        Debug.Log("对话序列结束.");
    }
}