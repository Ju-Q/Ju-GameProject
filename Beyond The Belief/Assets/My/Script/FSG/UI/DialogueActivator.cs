using System.Collections.Generic;
using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    [Header("引用 DialogueManager")]
    public DialogueManager1 dialogueManager;

    [System.Serializable]
    public class ActivationEvent
    {
        public int dialogueIndex;
        public GameObject targetObject;
        public bool setActiveState = true;

        [HideInInspector] public bool hasTriggered = false; // ✅ 新增：单独记录每个事件是否已触发
    }

    [Header("触发设置")]
    public List<ActivationEvent> activationEvents = new List<ActivationEvent>();

    void Update()
    {
        if (dialogueManager == null) return;

        int currentIndex = GetCurrentDialogueIndex(dialogueManager);

        foreach (var ev in activationEvents)
        {
            if (!ev.hasTriggered && ev.dialogueIndex == currentIndex)
            {
                if (ev.targetObject != null)
                {
                    ev.targetObject.SetActive(ev.setActiveState);
                }

                ev.hasTriggered = true; // ✅ 防止重复触发此条
            }
        }
    }

    int GetCurrentDialogueIndex(DialogueManager1 manager)
    {
        var field = typeof(DialogueManager1).GetField("currentDialogueIndex",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field.GetValue(manager);
    }
}
