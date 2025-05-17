using UnityEngine;

public class ItemPickupManager : MonoBehaviour
{
    public int propACount = 0; // ��ʰȡ��PropA����

    [Header("ʰȡ����")]
    public float pickupRange = 3f;        // ʰȡ����
    public LayerMask pickupLayerMask;    // ʰȡ��Ʒ�����Ĳ㼶����ѡ��

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
                // ʰȡ PropA ����
                propACount = Mathf.Min(propACount + 1, 3); // �������Ϊ3
                Debug.Log("ʰȡ��һ�� PropA������������" + propACount);
                Destroy(target); // ���Ϊ target.SetActive(false);
                return; // ֻʰȡһ������
            }
            else if (target.CompareTag("PropB"))
            {
                // ʰȡ PropB ���ߣ��˴������������߼���
                Debug.Log("ʰȡ��һ�� PropB");
                Destroy(target); // ���Ϊ target.SetActive(false);
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
