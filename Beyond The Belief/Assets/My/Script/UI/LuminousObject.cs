using UnityEngine;

public class LuminousObject : MonoBehaviour
{
    public Material luminousMaterial;

    private void Start()
    {
        // Check if the material is assigned
        if (luminousMaterial != null)
        {
            // Assign the luminous material to the object's renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = luminousMaterial;
            }
        }
        else
        {
            Debug.LogWarning("Luminous Material not assigned to LuminousObject.");
        }
    }
}
