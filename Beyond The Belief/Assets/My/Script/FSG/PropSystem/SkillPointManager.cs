using UnityEngine;
using UnityEngine.UI;

public class SkillPointManager : MonoBehaviour
{
    public static SkillPointManager Instance;

    [Header("���ܵ�����")]
    public int maxSkillPoints = 3;
    public int currentSkillPoints = 0;

    [Header("���ܵ�ͼ��")]
    public Image[] skillPointImages; // ��Inspector��ָ��3��Image

    [Header("�����ͷ�����")]
    [Tooltip("��ס��������������ͷż���")]
    public float holdTime = 2f;

    private float holdTimer = 0f;

    private void Awake()
    {
        // ����ģʽ
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
            Debug.Log("ʹ����һ�����ܵ㡣ʣ�༼�ܵ㣺" + currentSkillPoints);
            UpdateSkillPointUI();
            // TODO����Ӽ����ͷŵ�ʵ��Ч���߼�
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
