using UnityEngine;
using UnityEngine.UI;

public class SkillPointManager : MonoBehaviour
{
    public static SkillPointManager Instance;

    [Header("技能点设置")]
    public int maxSkillPoints = 3;
    public int currentSkillPoints = 0;

    [Header("技能点图标")]
    public Image[] skillPointImages; // 在Inspector中指定3个Image

    [Header("技能释放设置")]
    [Tooltip("按住鼠标左键多少秒后释放技能")]
    public float holdTime = 2f;

    private float holdTimer = 0f;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateSkillPointUI();
    }

    private void Update()
    {
        if (currentSkillPoints > 0 && Input.GetMouseButton(0))
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdTime)
            {
                UseSkillPoint();
                holdTimer = 0f;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
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

    public void SetSkillPoints(int count)
    {
        currentSkillPoints = Mathf.Clamp(count, 0, maxSkillPoints);
        UpdateSkillPointUI();
    }

    private void UseSkillPoint()
    {
        if (currentSkillPoints > 0)
        {
            currentSkillPoints--;
            Debug.Log("使用了一个技能点。剩余技能点：" + currentSkillPoints);
            UpdateSkillPointUI();
            // TODO：添加技能释放的实际效果逻辑
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
