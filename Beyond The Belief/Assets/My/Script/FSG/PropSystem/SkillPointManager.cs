using UnityEngine;
using UnityEngine.UI;

public class SkillPointManager : MonoBehaviour
{
    public static SkillPointManager Instance;

    public int currentSkillPoints = 0;
    public int maxSkillPoints = 3;
    public Image[] skillPointImages;

    private float holdTime = 2f;
    private float holdTimer = 0f;
    private bool isHolding = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            isHolding = true;
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                UseSkillPoint();
                holdTimer = 0f;
                isHolding = false;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isHolding = false;
            holdTimer = 0f;
        }
    }

    public void AddSkillPoint()
    {
        if (currentSkillPoints < maxSkillPoints)
        {
            currentSkillPoints++;
            UpdateSkillPointUI();
        }
    }

    public void SetSkillPoints(int value)
    {
        currentSkillPoints = Mathf.Clamp(value, 0, maxSkillPoints);
        UpdateSkillPointUI();
    }

    private void UseSkillPoint()
    {
        if (currentSkillPoints > 0)
        {
            currentSkillPoints--;
            UpdateSkillPointUI();
            Debug.Log("ʹ����һ�����ܵ㣬ʣ�ࣺ" + currentSkillPoints);
            // ���������ͷ�Ч���߼��ɼ�������
        }
    }

    private void UpdateSkillPointUI()
    {
        for (int i = 0; i < skillPointImages.Length; i++)
        {
            skillPointImages[i].enabled = i < currentSkillPoints;
        }
    }
}
