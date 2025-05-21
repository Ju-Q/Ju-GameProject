using UnityEngine;

public class ProximityIndicatorController : MonoBehaviour
{
    public float detectionRadius = 3f;
    public LayerMask interactableLayer; // Ö¸¶¨Layer

    private InteractableIndicator currentTarget;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);

        InteractableIndicator closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            InteractableIndicator indicator = hit.GetComponent<InteractableIndicator>();
            if (indicator != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    closest = indicator;
                    minDistance = dist;
                }
            }
        }

        // ÇÐ»»ÏÔÊ¾×´Ì¬
        if (closest != currentTarget)
        {
            if (currentTarget != null)
                currentTarget.HideIndicator();

            currentTarget = closest;

            if (currentTarget != null)
                currentTarget.ShowIndicator();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
