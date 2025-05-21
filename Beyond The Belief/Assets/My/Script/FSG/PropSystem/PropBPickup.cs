using UnityEngine;

public class PropBPickup : MonoBehaviour
{
    public SkillPointManager skillPointManager;


    void Start()
    {
        skillPointManager = FindObjectOfType<SkillPointManager>();
        if (skillPointManager == null)
        {
            skillPointManager = FindObjectOfType<SkillPointManager>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            skillPointManager?.AddSkillPoint();
            Destroy(gameObject);
        }
    }
}
