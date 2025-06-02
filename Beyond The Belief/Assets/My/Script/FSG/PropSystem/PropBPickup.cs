using UnityEngine;

// ����ű����ڴ������ʰȡ����B���߼�
public class PropBPickup : MonoBehaviour
{
    // ���� SkillPointManager �ű������ڹ����ܵ�
    public SkillPointManager skillPointManager;

    // ��Ϸ��ʼʱ�Զ�����
    void Start()
    {
        // �ڳ����в��� SkillPointManager ��ʵ������ֵ�� skillPointManager
        skillPointManager = FindObjectOfType<SkillPointManager>();

        // �����һ�β���ʧ�ܣ��ٳ��Բ���һ�Σ������е����࣬�����Ż���
        if (skillPointManager == null)
        {
            skillPointManager = FindObjectOfType<SkillPointManager>();
        }
    }

    // ����ҽ��봥�����򲢰��� F ��ʱִ��
    private void OnTriggerStay(Collider other)
    {
        // �����ײ�����Ƿ�����ң�Tag Ϊ "Player"�������Ƿ��� F ��
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            // ��� skillPointManager ��Ϊ�գ����� AddSkillPoint() �������Ӽ��ܵ�
            skillPointManager?.AddSkillPoint();

            // ע�͵��Ĵ��룺ʰȡ�����ٵ��ߣ����Ը�������ȡ��ע�ͣ�
            // Destroy(gameObject);
        }
    }
}