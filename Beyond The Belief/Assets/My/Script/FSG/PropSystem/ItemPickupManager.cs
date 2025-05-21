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
        // 当玩家按下F键时尝试拾取
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
                    Debug.Log("首次拾取了 PropA，获得3点技能点");

                    if (SkillPointManager.Instance != null)
                    {
                        SkillPointManager.Instance.SetSkillPoints(3);
                    }
                }
                else
                {
                    Debug.Log("已经拾取过 PropA，无法再次拾取");
                }

                Destroy(target); // 或改为 target.SetActive(false);
                return; // 只拾取一个道具
            }
            else if (target.CompareTag("PropB"))
            {
                // 如果已经通过 PropA 获得了技能点才能拾取 PropB
                if (!hasPickedPropA)
                {
                    Debug.Log("请先拾取 PropA 才能拾取 PropB");
                    return;
                }

                // 拾取 PropB 道具并添加技能点（上限为3）
                if (SkillPointManager.Instance != null && SkillPointManager.Instance.currentSkillPoints < SkillPointManager.Instance.maxSkillPoints)
                {
                    Debug.Log("拾取了一个 PropB");
                    SkillPointManager.Instance.AddSkillPoint();
                    Destroy(target); // 或改为 target.SetActive(false);
                }
                else
                {
                    Debug.Log("PropB 技能点已满，无法拾取。");
                }
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 可视化拾取范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
