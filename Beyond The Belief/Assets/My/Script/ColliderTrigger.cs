using System.Collections;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    public GameObject ShowCanvas;
    public float delaySeconds = 2.0f; // Time to wait before starting fade out
    public float fadeDuration = 1.0f; // Duration of fade in and fade out

    private CanvasGroup canvasGroup;
    private bool hasTriggered = false; // Track if the trigger has already been activated

    void Start()
    {
        // Add a CanvasGroup component to the ShowCanvas if it doesn't already have one
        canvasGroup = ShowCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = ShowCanvas.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0; // Start fully transparent
        ShowCanvas.SetActive(false); // Start inactive
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true; // Mark the trigger as used
            ShowCanvas.SetActive(true); // Activate the canvas
            StartCoroutine(FadeIn()); // Start the fade-in effect
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration); // Fade in
            yield return null;
        }
        StartCoroutine(WaitAndFadeOut()); // Start the fade-out after delaySeconds
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(delaySeconds); // Wait for the specified delay
        StartCoroutine(FadeOut()); // Start the fade-out effect
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration)); // Fade out
            yield return null;
        }
        ShowCanvas.SetActive(false); // Deactivate the canvas after fade-out is complete
    }
}
