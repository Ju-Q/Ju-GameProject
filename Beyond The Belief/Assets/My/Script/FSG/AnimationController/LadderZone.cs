using UnityEngine;

public class LadderZone : MonoBehaviour
{
    public Transform climbStartPoint;
    public Transform climbEndPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLadderClimb climb = other.GetComponent<PlayerLadderClimb>();
            if (climb != null)
            {
                climb.EnterLadder(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLadderClimb climb = other.GetComponent<PlayerLadderClimb>();
            if (climb != null)
            {
                climb.ExitLadder();
            }
        }
    }
}
