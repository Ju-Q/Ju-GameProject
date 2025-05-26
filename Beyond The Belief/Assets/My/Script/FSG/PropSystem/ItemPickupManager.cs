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
                hasPickedPropA = true;
                propACount++;

                if (SkillPointManager.Instance != null)
                {
                    if (propACount >= 3)
                    {
                        SkillPointManager.Instance.maxSkillPoints = 6;
                        SkillPointManager.Instance.SetSkillPoints(6);
                        Debug.Log("ʰȡ�˵�3�� PropA�����ܵ����������6���������");
                    }
                    else
                    {
                        SkillPointManager.Instance.maxSkillPoints = 3;
                        SkillPointManager.Instance.SetSkillPoints(3);
                        Debug.Log("ʰȡ�� PropA�����ܵ�ָ�Ϊ3��");
                    }
                }

                Destroy(target);
                return;
            }
            else if (target.CompareTag("PropB"))
            {
                if (!hasPickedPropA)
                {
                    Debug.Log("����ʰȡ PropA ����ʰȡ PropB");
                    return;
                }

                if (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints < SkillPointManager.Instance.maxSkillPoints)
                {
                    SkillPointManager.Instance.AddSkillPoint();
                    Debug.Log("ʰȡ��һ�� PropB");
                    Destroy(target);
                }
                else
                {
                    Debug.Log("PropB ���ܵ��������޷�ʰȡ");
                }
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
