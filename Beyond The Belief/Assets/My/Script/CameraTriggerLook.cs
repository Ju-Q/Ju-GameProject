using UnityEngine;
using System.Collections;

public class CameraTriggerLook : MonoBehaviour
{
    public Transform npc;              // The transform of the NPC
    public Camera mainCamera;          // Reference to the main camera
    public float lookDuration = 2.0f;  // Time to look at the NPC
    public float cameraSpeed = 2.0f;   // Speed of camera movement
    public Vector3 offsetFromNPC = new Vector3(0, 2, -5);  // Offset position when looking at NPC

    private bool hasTriggered = false;  // Prevents the trigger from being used more than once
    private Vector3 savedLocalPosition; // Camera's local position relative to the player
    private Quaternion savedLocalRotation; // Camera's local rotation relative to the player
    private Transform cameraParent;    // Original parent of the camera

    void Start()
    {
        // Save the original parent of the camera
        cameraParent = mainCamera.transform.parent;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            // Save the camera's local position and rotation relative to its parent (the player)
            savedLocalPosition = mainCamera.transform.localPosition;
            savedLocalRotation = mainCamera.transform.localRotation;

            hasTriggered = true; // Mark the trigger as used
            StartCoroutine(LookAtNPC());
        }
    }

    IEnumerator LookAtNPC()
    {
        // Step 1: Detach the camera from the player to freely control its transform
        mainCamera.transform.SetParent(null);

        Vector3 targetPosition = npc.position + offsetFromNPC; // Calculate the target position
        Quaternion targetRotation = Quaternion.LookRotation(npc.position - targetPosition);

        // Smoothly move the camera to the target position and rotation
        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.1f ||
               Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * cameraSpeed);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);
            yield return null;
        }

        // Step 2: Wait for a specified number of seconds
        yield return new WaitForSeconds(lookDuration);

        // Step 3: Smoothly return the camera to its saved local position and rotation relative to the player
        Vector3 targetReturnPosition = cameraParent.TransformPoint(savedLocalPosition); // Convert local to world position
        Quaternion targetReturnRotation = cameraParent.rotation * savedLocalRotation;  // Convert local to world rotation

        while (Vector3.Distance(mainCamera.transform.position, targetReturnPosition) > 0.1f ||
               Quaternion.Angle(mainCamera.transform.rotation, targetReturnRotation) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetReturnPosition, Time.deltaTime * cameraSpeed);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetReturnRotation, Time.deltaTime * cameraSpeed);
            yield return null;
        }

        // Step 4: Reparent the camera back to the player
        mainCamera.transform.SetParent(cameraParent);

        // Ensure the camera retains its original local position and rotation relative to the player
        mainCamera.transform.localPosition = savedLocalPosition;
        mainCamera.transform.localRotation = savedLocalRotation;
    }
}
