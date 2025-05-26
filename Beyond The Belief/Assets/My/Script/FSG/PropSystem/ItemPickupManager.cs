using UnityEngine;

public class ItemPickupManager : MonoBehaviour
{
    public int propACount = 0; // 已拾取的PropA数量

    [Header("拾取设置")]
    public float pickupRange = 3f;        // 拾取距离
    public LayerMask pickupLayerMask;    // 拾取物品所属的层级（可选）

    private bool hasPickedPropA = false; // 是否已经拾取过 PropA

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
                        Debug.Log("拾取了第3个 PropA，技能点上限提高至6并获得满点");
                    }
                    else
                    {
                        SkillPointManager.Instance.maxSkillPoints = 3;
                        SkillPointManager.Instance.SetSkillPoints(3);
                        Debug.Log("拾取了 PropA，技能点恢复为3点");
                    }
                }

                Destroy(target);
                return;
            }
            else if (target.CompareTag("PropB"))
            {
                if (!hasPickedPropA)
                {
                    Debug.Log("请先拾取 PropA 才能拾取 PropB");
                    return;
                }

                if (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints < SkillPointManager.Instance.maxSkillPoints)
                {
                    SkillPointManager.Instance.AddSkillPoint();
                    Debug.Log("拾取了一个 PropB");
                    Destroy(target);
                }
                else
                {
                    Debug.Log("PropB 技能点已满，无法拾取");
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
