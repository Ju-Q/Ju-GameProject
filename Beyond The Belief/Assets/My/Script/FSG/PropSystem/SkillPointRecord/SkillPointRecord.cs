using UnityEngine;

public class SkillPointRecord : MonoBehaviour
{
    public static SkillPointRecord Instance;
    public int rememberedSkillPoints = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RememberSkillPoints(int value)
    {
        rememberedSkillPoints = value;
    }

    public int GetRememberedPoints()
    {
        return rememberedSkillPoints;
    }
}
