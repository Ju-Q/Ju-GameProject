using UnityEngine;

/// <summary>
/// �����뽻����ʾ����������������Χ�Ŀɽ������壬����ʾ����Ľ�����ʾ
/// </summary>
public class ProximityIndicatorController : MonoBehaviour
{
    [Header("�������")]
    public float detectionRadius = 3f;          // ���뾶
    public LayerMask interactableLayer;         // ָ���ɽ����������ڵ�Layer���������޹ض���

    private InteractableIndicator currentTarget; // ��ǰ����Ľ���Ŀ��

    private void Update()
    {
        // 1. ���μ�⣺��ȡ�뾶��������ָ��Layer�ϵ���ײ��
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);

        // 2. Ѱ������Ľ���Ŀ��
        InteractableIndicator closest = null;
        float minDistance = Mathf.Infinity;     // ��ʼ��Ϊ���޴󣬱��ڱȽ�

        foreach (var hit in hits)
        {
            // ���Դ���ײ���ȡInteractableIndicator���
            InteractableIndicator indicator = hit.GetComponent<InteractableIndicator>();
            if (indicator != null)
            {
                // ��������ҵľ���
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    closest = indicator;        // �������Ŀ��
                    minDistance = dist;         // ������С����
                }
            }
        }

        // 3. �л���ʾ״̬��������Ŀ�귢���仯
        if (closest != currentTarget)
        {
            // ���ؾ�Ŀ�����ʾ
            if (currentTarget != null)
                currentTarget.HideIndicator();

            // ���µ�ǰĿ��
            currentTarget = closest;

            // ��ʾ��Ŀ�����ʾ
            if (currentTarget != null)
                currentTarget.ShowIndicator();
        }
    }

    /// <summary>
    /// ��Unity�༭���л��Ƽ�ⷶΧ��Gizmo��������ԣ�
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}