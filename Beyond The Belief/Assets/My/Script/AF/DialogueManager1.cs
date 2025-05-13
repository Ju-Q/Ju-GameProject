using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager1 : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // Reference to the TextMeshProUGUI element
    public List<string> dialogues; // List to hold dialogue strings
    public Image characterPortrait; // Reference to the character portrait Image component
    public List<Sprite> characterPortraits; // List of character portraits
    public Image backgroundImage; // Reference to the background Image component
    public Sprite backgroundSprite; // Single background image for the whole conversation

    private int currentDialogueIndex = 0;
    private Coroutine typingCoroutine; // Reference to the current typing coroutine
    private bool isTyping = false; // Flag to track if typing is in progress

    void Start()
    {
        if (dialogues.Count > 0)
        {
            // Hide the background and portrait initially
            backgroundImage.enabled = false;
            characterPortrait.enabled = false;
            UpdateDialogueUI();
        }
        else
        {
            Debug.LogWarning("No dialogues available.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Press Enter to show the next dialogue
        {
            ShowNextDialogue();
        }
    }

    // Show the next dialogue in the list
    void ShowNextDialogue()
    {
        if (isTyping) // If typing is in progress
        {
            // Immediately display the full text and stop the typing effect
            StopTypingEffect();
        }
        else if (currentDialogueIndex < dialogues.Count - 1) // Move to the next dialogue
        {
            currentDialogueIndex++;
            UpdateDialogueUI();
        }
        else
        {
            // Dialogue finished, hide everything
            EndDialogue();
        }
    }

    // Update the dialogue text and character portrait
    void UpdateDialogueUI()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // Stop any ongoing typing coroutine
        }

        typingCoroutine = StartCoroutine(TypeDialogue(dialogues[currentDialogueIndex])); // Start new typing effect

        // Set the character portrait for the current dialogue
        if (currentDialogueIndex < characterPortraits.Count)
        {
            characterPortrait.sprite = characterPortraits[currentDialogueIndex];
            characterPortrait.enabled = true; // Show the character portrait
        }
        else
        {
            characterPortrait.enabled = false; // Hide if there is no corresponding portrait
        }

        // Set the background image for the entire conversation
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.enabled = true; // Show the background image
    }

    // Typewriter effect for displaying dialogue text
    IEnumerator TypeDialogue(string text)
    {
        isTyping = true; // Set typing flag
        dialogueText.text = ""; // Clear existing text
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Slower typing speed (0.05 seconds per letter)
        }
        isTyping = false; // Typing complete
    }

    // Immediately complete the typing effect
    void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // Stop the coroutine
        }

        dialogueText.text = dialogues[currentDialogueIndex]; // Display full dialogue text
        isTyping = false; // Reset typing flag
    }

    // End the dialogue sequence (when there are no more dialogues)
    void EndDialogue()
    {
        dialogueText.text = ""; // Optionally display "The End" or some other message
        backgroundImage.enabled = false; // Hide the background image
        characterPortrait.enabled = false; // Hide the character portrait
        Debug.Log("Dialogue sequence finished.");
    }
}
