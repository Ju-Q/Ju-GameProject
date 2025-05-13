using UnityEngine;
using TMPro; // Make sure TextMeshPro is included

public class DialogueTrigger : MonoBehaviour
{
    public GameObject textMeshProObject; // Reference to the TextMeshPro GameObject

    private bool isPlayerInRange = false; // Tracks if the player is within range
    private SphereCollider sphereCollider; // Collider for the trigger

    void Start()
    {
        // Automatically add a SphereCollider and set it as a trigger
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;

        // Ensure the TextMeshPro GameObject is assigned
        if (textMeshProObject == null)
        {
            //Debug.LogError("TextMeshPro GameObject is not assigned in the inspector.");
            return;
        }

        // Deactivate the TextMeshPro GameObject at the start
        textMeshProObject.SetActive(false);
    }

    void Update()
    {
        // Check if the player is in range and the F key is pressed
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // Activate the TextMeshPro GameObject when the player presses F
            textMeshProObject.SetActive(true);
            Debug.Log("F key pressed. Displaying dialogue.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered the trigger area.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the object exiting the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left the trigger area.");

            // Deactivate the TextMeshPro GameObject when the player leaves the trigger area
            textMeshProObject.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the trigger collider in the editor for debugging purposes
        if (sphereCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        }
    }
}
