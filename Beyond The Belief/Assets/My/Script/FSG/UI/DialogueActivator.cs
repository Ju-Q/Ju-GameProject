using System.Collections.Generic;
using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    [Header("引用 DialogueManager")]
    public DialogueManager1 dialogueManager;

    [System.Serializable]
    public class ActivationEvent
    {
        public int dialogueIndex;       // 第几句话时触发（从0开始）
        public GameObject targetObject; // 要激活的物体
        public bool setActiveState = true; // 激活还是隐藏
    }

    [Header("触发设置")]
    public List<ActivationEvent> activationEvents = new List<ActivationEvent>();

    private HashSet<int> triggeredIndices = new HashSet<int>(); // 避免重复触发

    void Update()
    {
        if (dialogueManager == null || !dialogueManager.isDialogueActive) return;

        int currentIndex = GetCurrentDialogueIndex(dialogueManager);
        foreach (var ev in activationEvents)
        {
            if (ev.dialogueIndex == currentIndex && !triggeredIndices.Contains(currentIndex))
            {
                if (ev.targetObject != null)
                {
                    ev.targetObject.SetActive(ev.setActiveState);
                }
                triggeredIndices.Add(currentIndex);
            }
        }
    }

    // 用反射安全访问 private 的 currentDialogueIndex
    int GetCurrentDialogueIndex(DialogueManager1 manager)
    {
        var field = typeof(DialogueManager1).GetField("currentDialogueIndex",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field.GetValue(manager);
    }
}
