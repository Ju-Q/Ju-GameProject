using UnityEngine;

public class ItemPickupManager : MonoBehaviour
{
    public int propACount = 0; // ��ʰȡ��PropA����

    [Header("ʰȡ����")]
    public float pickupRange = 3f;        // ʰȡ����
    public LayerMask pickupLayerMask;    // ʰȡ��Ʒ�����Ĳ㼶����ѡ��

    private bool hasPickedPropA = false; // �Ƿ��Ѿ�ʰȡ�� PropA

    private void Update()
    {
        // ����Ұ���F��ʱ����ʰȡ
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickupItem();
        }
    }

    private void TryPickupItem()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        foreach (Collider hit in hits)
        {
            GameObject target = hit.gameObject;

            if (target.CompareTag("PropA"))
            {
                if (!hasPickedPropA)
                {
                    hasPickedPropA = true;
                    propACount = 1;
                    Debug.Log("�״�ʰȡ�� PropA�����3�㼼�ܵ�");

                    if (SkillPointManager.Instance != null)
                    {
                        SkillPointManager.Instance.SetSkillPoints(3);
                    }
                }
                else
                {
                    Debug.Log("�Ѿ�ʰȡ�� PropA���޷��ٴ�ʰȡ");
                }

                Destroy(target); // ���Ϊ target.SetActive(false);
                return; // ֻʰȡһ������
            }
            else if (target.CompareTag("PropB"))
            {
                // ����Ѿ�ͨ�� PropA ����˼��ܵ����ʰȡ PropB
                if (!hasPickedPropA)
                {
                    Debug.Log("����ʰȡ PropA ����ʰȡ PropB");
                    return;
                }

                // ʰȡ PropB ���߲���Ӽ��ܵ㣨����Ϊ3��
                if (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints < SkillPointManager.Instance.maxSkillPoints)
                {
                    Debug.Log("ʰȡ��һ�� PropB");
                    SkillPointManager.Instance.AddSkillPoint();
                    Destroy(target); // ���Ϊ target.SetActive(false);
                }
                else
                {
                    Debug.Log("PropB ���ܵ��������޷�ʰȡ��");
                }
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ���ӻ�ʰȡ��Χ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
