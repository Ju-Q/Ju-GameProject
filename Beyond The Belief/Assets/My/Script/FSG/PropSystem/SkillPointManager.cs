using UnityEngine;
using UnityEngine.UI;

// ���ܵ�������������ܵ�����ӡ����ġ�UI���¼��������
public class SkillPointManager : MonoBehaviour
{
    // ====== ����ģʽ ======
    public static SkillPointManager Instance; // ȫ��Ψһʵ�������������ű�����

    // ====== ���ܵ����� ======
    public int currentSkillPoints = 0;  // ��ǰӵ�еļ��ܵ���
    public int maxSkillPoints = 3;     // ���ܵ�����
    public Image[] skillPointImages;   // UI��ʾ�ļ��ܵ�ͼ�꣨��3��Image��ɵ����飩

    // ====== ���������� ======
    private float holdTime = 2f;       // ��������������ʱ�䣨�룩
    private float holdTimer = 0f;      // ��ʱ������¼��������ס��ʱ��
    private bool isHolding = false;    // �Ƿ����ڳ���

    // ====== ��ʼ������ ======
    private void Awake()
    {
        if (Instance == null)
            Instance = this; // ȷ��������ֻ��һ��ʵ��
    }

    // ====== ÿ֡��ⳤ���߼� ======
    private void Update()
    {
        if (Input.GetMouseButton(0)) // ��������סʱ
        {
            isHolding = true;
            holdTimer += Time.deltaTime; // �ۼӰ�סʱ��

            // �����סʱ�䳬����ֵ���������ܵ�����
            if (holdTimer >= holdTime)
            {
                UseSkillPoint();    // ���ļ��ܵ�
                holdTimer = 0f;       // ���ü�ʱ��
                isHolding = false;    // ��������״̬
            }
        }
        else if (Input.GetMouseButtonUp(0)) // �������ɿ�ʱ
        {
            isHolding = false;
            holdTimer = 0f; // ���ü�ʱ��
        }
    }

    // ====== ���Ӽ��ܵ� ======
    public void AddSkillPoint()
    {
        if (currentSkillPoints < maxSkillPoints)
        {
            currentSkillPoints++;
            UpdateSkillPointUI(); // ����UI��ʾ
        }
    }

    // ====== ǿ�����ü��ܵ���ֵ ======
    public void SetSkillPoints(int value)
    {
        // ������ֵ��0��maxSkillPoints֮��
        currentSkillPoints = Mathf.Clamp(value, 0, maxSkillPoints);
        UpdateSkillPointUI();
    }

    // ====== ���ļ��ܵ� ======
    private void UseSkillPoint()
    {
        if (currentSkillPoints > 0)
        {
            currentSkillPoints--;
            UpdateSkillPointUI();
            Debug.Log("ʹ����һ�����ܵ㣬ʣ�ࣺ" + currentSkillPoints);
            // ���������Ӽ����ͷŵľ����߼����粥����Ч���������ܵȣ�
        }
    }

    // ====== ����UI��ʾ ======
    private void UpdateSkillPointUI()
    {
        // �������м��ܵ�ͼ�꣬���ݵ�ǰ���ܵ�����ʾ/����
        for (int i = 0; i < skillPointImages.Length; i++)
        {
            skillPointImages[i].enabled = i < currentSkillPoints;
            // ���磺currentSkillPoints=2ʱ��ǰ����ͼ����ʾ������������
        }
    }
}