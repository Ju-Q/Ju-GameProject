using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public List<string> content; // List of dialogue strings
    public List<Texture> speakerImages; // List of speaker images matching dialogue sequence
}

public class ButtonSelector : MonoBehaviour
{
    public GameObject canvas;

    // Public Button variables for each button
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    // Public TMP_Text variables for each button's dialogue display
    public TMP_Text dialogueText1;
    public TMP_Text dialogueText2;
    public TMP_Text dialogueText3;
    public TMP_Text dialogueText4;

    // Public RawImage variables for speaker identity photos
    public RawImage speakerImage1;
    public RawImage speakerImage2;
    public RawImage speakerImage3;
    public RawImage speakerImage4;

    public List<Dialogue> dialogues; // List of dialogues for each button
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

    void Start()
    {
        canvas.SetActive(false);

        // Ensure Canvas is on top
        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            canvasComponent.sortingOrder = 10; // Set a higher sorting order for this canvas
        }

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
        Button[] buttons = { button1, button2, button3, button4 };

        for (int i = 0; i < buttons.Length; i++)
        {
            ColorBlock colors = buttons[i].colors;
            colors.normalColor = (i == selectedButton) ? selectedButtonColor : Color.white;
            buttons[i].colors = colors;
        }
    }

    private IEnumerator DisplayDialogueWithTypingEffect(Dialogue dialogue)
    {
        TMP_Text currentDialogueText = null;
        RawImage currentSpeakerImage = null;

        // Determine which dialogue text and speaker image to use
        switch (selectedButton)
        {
            case 0:
                currentDialogueText = dialogueText1;
                currentSpeakerImage = speakerImage1;
                break;
            case 1:
                currentDialogueText = dialogueText2;
                currentSpeakerImage = speakerImage2;
                break;
            case 2:
                currentDialogueText = dialogueText3;
                currentSpeakerImage = speakerImage3;
                break;
            case 3:
                currentDialogueText = dialogueText4;
                currentSpeakerImage = speakerImage4;
                break;
        }

        currentDialogueIndex = 0; // Reset index at the start
        currentDialogueText.text = ""; // Clear previous dialogue text

        while (true)
        {
            // Check if speaker image is available for the current index
            if (currentSpeakerImage != null && currentDialogueIndex < dialogue.speakerImages.Count)
            {
                currentSpeakerImage.texture = dialogue.speakerImages[currentDialogueIndex];
                currentSpeakerImage.gameObject.SetActive(true);
            }
            else if (currentSpeakerImage != null)
            {
                currentSpeakerImage.gameObject.SetActive(false); // Hide if no image is available
            }

            // Display the current dialogue line with typing effect
            string currentLine = dialogue.content[currentDialogueIndex];
            yield return StartCoroutine(TypeText(currentDialogueText, currentLine));

            // Wait for user input to proceed to the next dialogue line
            while (true)
            {
                if (Input.GetKeyDown(nextDialogueKey))
                {
                    currentDialogueIndex++;

                    // Check if we're done with all dialogue lines
                    if (currentDialogueIndex >= dialogue.content.Count)
                    {
                        currentDialogueText.text = ""; // Clear text
                        if (currentSpeakerImage != null)
                            currentSpeakerImage.gameObject.SetActive(false); // Hide image

                        isDisplayingDialogue = false; // Reset flag to allow new dialogue
                        yield break; // End coroutine
                    }
                    break; // Proceed to next line
                }
                yield return null;
            }
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
        // Ensure the buttons and speaker images are not null before hiding
        if (button1 != null) button1.gameObject.SetActive(false);
        if (button2 != null) button2.gameObject.SetActive(false);
        if (button3 != null) button3.gameObject.SetActive(false);
        if (button4 != null) button4.gameObject.SetActive(false);

        // Hide all speaker images
        if (speakerImage1 != null) speakerImage1.gameObject.SetActive(false);
        if (speakerImage2 != null) speakerImage2.gameObject.SetActive(false);
        if (speakerImage3 != null) speakerImage3.gameObject.SetActive(false);
        if (speakerImage4 != null) speakerImage4.gameObject.SetActive(false);
    }

    private void ShowAllButtons()
    {
        if (button1 != null) button1.gameObject.SetActive(true);
        if (button2 != null) button2.gameObject.SetActive(true);
        if (button3 != null) button3.gameObject.SetActive(true);
        if (button4 != null) button4.gameObject.SetActive(true);
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
