using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ButtonDialogue
{
    public List<string> content;          // List of dialogue strings
    public List<Texture> speakerImages;   // List of speaker images matching dialogue sequence
    public Texture backgroundImage;       // Background image for this dialogue
}

public class ButtonSelector1 : MonoBehaviour
{
    public GameObject canvas;

    // Public lists for buttons, dialogue text, and speaker images
    public List<Button> buttons;            // List of buttons
    public List<TMP_Text> dialogueTexts;    // List of TMP_Text for each button's dialogue display
    public List<RawImage> speakerImages;    // List of RawImages for speaker identity photos

    public List<ButtonDialogue> dialogues; // List of dialogues for each button (renamed to ButtonDialogue)
    private int selectedButton = 0;
    private int currentDialogueIndex = 0; // Track which dialogue line is currently displayed
    private bool isInTrigger = false; // Track if the player is in the trigger area
    private bool isDisplayingDialogue = false; // Flag to prevent multiple dialogues from being shown at once

    // Public key assignments
    public KeyCode toggleKey = KeyCode.G; // Key to toggle the canvas
    public KeyCode selectLeftKey = KeyCode.A; // Key to select the left button
    public KeyCode selectRightKey = KeyCode.D; // Key to select the right button
    public KeyCode confirmKey = KeyCode.Return; // Key to confirm selection
    public KeyCode nextDialogueKey = KeyCode.Space; // Key to move to the next dialogue entry

    // Public color for the selected button
    public Color selectedButtonColor = Color.yellow; // Color for the selected button

    // Typing effect settings
    public float typingSpeed = 0.05f; // Speed of the typing effect

    // Add a RawImage for the background
    public RawImage backgroundImage;  // Reference to the background image RawImage

    void Start()
    {
        canvas.SetActive(false);

        // Ensure Canvas is on top
        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.sortingOrder = 10; // Set a higher sorting order for this canvas
        }

        // Hide speaker images initially
        HideAllSpeakerImages();

        UpdateButtonSelection();
    }

    void Update()
    {
        if (isInTrigger)
        {
            if (Input.GetKeyDown(toggleKey))
            {
                canvas.SetActive(!canvas.activeSelf);
                if (canvas.activeSelf)
                {
                    currentDialogueIndex = 0;
                    UpdateButtonSelection();
                    ShowAllButtons();
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
                    isDisplayingDialogue = true; // Prevent multiple dialogues from being displayed at the same time
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

        // Set background image based on the selected dialogue
        if (backgroundImage != null)
        {
            backgroundImage.texture = dialogue.backgroundImage; // Set the background image for the dialogue
            backgroundImage.gameObject.SetActive(true); // Ensure the background is visible during dialogue
        }

        // Determine which dialogue text and speaker image to use
        if (selectedButton >= 0 && selectedButton < dialogueTexts.Count)
        {
            currentDialogueText = dialogueTexts[selectedButton];
            currentSpeakerImage = speakerImages[selectedButton];
        }

        currentDialogueIndex = 0; // Reset index at the start
        currentDialogueText.text = ""; // Clear previous dialogue text

        // **Immediately show the speaker image** when the first dialogue starts
        if (currentSpeakerImage != null && currentDialogueIndex < dialogue.speakerImages.Count)
        {
            currentSpeakerImage.texture = dialogue.speakerImages[currentDialogueIndex];
            currentSpeakerImage.gameObject.SetActive(true); // Show the first speaker image right away
        }

        // Display the first line of dialogue with typing effect
        string currentLine = dialogue.content[currentDialogueIndex];
        yield return StartCoroutine(TypeText(currentDialogueText, currentLine));

        // Wait for user input to proceed to the next dialogue line
        while (currentDialogueIndex < dialogue.content.Count)
        {
            // Check for "next dialogue" key input
            if (Input.GetKeyDown(nextDialogueKey))
            {
                currentDialogueIndex++;

                // Check if we're done with all dialogue lines
                if (currentDialogueIndex >= dialogue.content.Count)
                {
                    currentDialogueText.text = ""; // Clear text
                    if (currentSpeakerImage != null)
                        currentSpeakerImage.gameObject.SetActive(false); // Hide image

                    if (backgroundImage != null)
                    {
                        backgroundImage.texture = null; // Clear the background image
                        backgroundImage.gameObject.SetActive(false); // Hide the background image after the dialogue ends
                    }

                    isDisplayingDialogue = false; // Reset flag to allow new dialogue
                    HideAllButtons(); // Hide buttons after dialogue ends
                    canvas.SetActive(false); // Hide the entire canvas
                    yield break; // End coroutine
                }
                else
                {
                    // Show the speaker image when the next dialogue line appears
                    if (currentSpeakerImage != null && currentDialogueIndex < dialogue.speakerImages.Count)
                    {
                        currentSpeakerImage.texture = dialogue.speakerImages[currentDialogueIndex];
                        currentSpeakerImage.gameObject.SetActive(true); // Show speaker image for this dialogue line
                    }

                    // Display the next line of dialogue with typing effect
                    currentLine = dialogue.content[currentDialogueIndex];
                    yield return StartCoroutine(TypeText(currentDialogueText, currentLine));
                }
            }
            yield return null;
        }
    }

    private IEnumerator TypeText(TMP_Text dialogueText, string line)
    {
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void HideAllButtons()
    {
        // Hide all buttons
        foreach (var button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
    }

    private void ShowAllButtons()
    {
        // Show all buttons
        foreach (var button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(true);
        }
    }

    private void HideAllSpeakerImages()
    {
        // Hide all speaker images at the start or when selecting buttons
        foreach (var speakerImage in speakerImages)
        {
            if (speakerImage != null)
                speakerImage.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = true;
            canvas.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = false;
            canvas.SetActive(false);
        }
    }
}
