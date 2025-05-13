using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // Reference to the TextMeshProUGUI element
    public List<string> dialogues; // List to hold dialogue strings
    public Image backgroundImage; // Reference to the background Image component
    public List<Sprite> backgroundImages; // List of background images corresponding to dialogues

    private int currentDialogueIndex = 0;
    private bool autoTriggerNextDialogue = false;
    private int nextDialogueIndex = -1;

    // Dictionary to map specific dialogue indices to background image indices
    public Dictionary<int, int> dialogueToBackgroundMapping = new Dictionary<int, int>();

    public bool autoAdvanceDialogue = false; // Option to auto advance dialogues
    public float dialogueDelay = 3f; // Delay between automatic dialogue advancement

    void Start()
    {
        if (dialogues.Count > 0)
        {
            // Ensure the background is hidden at the start if not mapped to the first dialogue
            backgroundImage.enabled = false;
            UpdateDialogueUI(currentDialogueIndex);

            if (autoAdvanceDialogue)
            {
                StartCoroutine(AutoAdvanceDialogue());
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Press Enter to show next dialogue
        {
            ShowNextDialogue();
        }

        if (Input.GetKeyDown(KeyCode.Backspace)) // Press Backspace to go to previous dialogue
        {
            ShowPreviousDialogue();
        }
    }

    // Method to display the next dialogue in the list
    void ShowNextDialogue()
    {
        if (currentDialogueIndex < dialogues.Count - 1)
        {
            currentDialogueIndex++;
            UpdateDialogueUI(currentDialogueIndex);
        }
        else if (autoTriggerNextDialogue && nextDialogueIndex >= 0)
        {
            StartDialogue(nextDialogueIndex);
        }
        else
        {
            dialogueText.text = ""; // Clear the text when dialogues are finished
        }
    }

    // Method to display the previous dialogue in the list
    void ShowPreviousDialogue()
    {
        if (currentDialogueIndex > 0)
        {
            currentDialogueIndex--;
            UpdateDialogueUI(currentDialogueIndex);
        }
    }

    // Method to start a specific dialogue
    public void StartDialogue(int index)
    {
        if (index >= 0 && index < dialogues.Count)
        {
            currentDialogueIndex = index;
            UpdateDialogueUI(currentDialogueIndex);
        }
    }

    // Method to set automatic trigger for the next dialogue after the current one finishes
    public void SetAutoTriggerNextDialogue(int nextIndex)
    {
        autoTriggerNextDialogue = true;
        nextDialogueIndex = nextIndex;
    }

    // Method to update the UI with the current dialogue and manage the background image
    void UpdateDialogueUI(int index)
    {
        StopAllCoroutines(); // Stops any currently running typewriter effect coroutine
        StartCoroutine(TypeDialogue(dialogues[index])); // Start typewriter effect for dialogue text

        // Check if the current dialogue index has a corresponding background image mapping
        if (dialogueToBackgroundMapping.ContainsKey(index))
        {
            int backgroundIndex = dialogueToBackgroundMapping[index];
            if (backgroundIndex >= 0 && backgroundIndex < backgroundImages.Count)
            {
                backgroundImage.sprite = backgroundImages[backgroundIndex];
                backgroundImage.enabled = true; // Show the background image
            }
        }
        else
        {
            // Hide the background if no mapping is found for the current dialogue
            backgroundImage.enabled = false;
        }
    }

    // Coroutine to display dialogue with a typewriter effect
    IEnumerator TypeDialogue(string text)
    {
        dialogueText.text = ""; // Clear the current dialogue text
        foreach (char letter in text.ToCharArray()) // Loop through each letter
        {
            dialogueText.text += letter; // Add each letter one by one
            yield return new WaitForSeconds(0.05f); // Adjust typing speed here
        }
    }

    // Coroutine for automatic dialogue advancement
    IEnumerator AutoAdvanceDialogue()
    {
        while (currentDialogueIndex < dialogues.Count - 1)
        {
            yield return new WaitForSeconds(dialogueDelay); // Wait for the specified delay
            ShowNextDialogue(); // Show the next dialogue after delay
        }
    }
}
