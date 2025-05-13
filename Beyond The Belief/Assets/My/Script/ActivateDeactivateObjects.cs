using UnityEngine;
using System.Collections.Generic;
using System.Collections; // 添加这一行以确保正确引用IEnumerator

public class ActivateDeactivateObjects : MonoBehaviour
{
    // Public list to hold the objects to activate
    public List<GameObject> objectsToControl;

    // Flag to ensure the trigger only fires once
    private bool hasTriggered = false;

    // Animation length in seconds (adjust according to your animation)
    public float animationDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the triggering object has the "Player" tag and the trigger hasn't fired yet
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true; // Mark as triggered

            // Activate all objects in the list
            foreach (GameObject obj in objectsToControl)
            {
                if (obj != null) obj.SetActive(true);
            }

            // Start coroutine to automatically deactivate objects after animation
            StartCoroutine(AutoDeactivateAfterAnimation());
        }
    }

    private IEnumerator AutoDeactivateAfterAnimation()
    {
        // Wait for the specified animation duration
        yield return new WaitForSeconds(animationDuration);

        // Deactivate all objects in the list
        foreach (GameObject obj in objectsToControl)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}
