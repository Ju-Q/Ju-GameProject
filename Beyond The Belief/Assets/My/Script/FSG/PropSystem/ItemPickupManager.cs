using UnityEngine;

public class ItemPickupManager : MonoBehaviour
{
    public int propACount = 0; // 已拾取的PropA数量

    [Header("拾取设置")]
    public float pickupRange = 3f;        // 拾取距离
    public LayerMask pickupLayerMask;    // 拾取物品所属的层级（可选）

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
                // 拾取 PropA 道具
                propACount = Mathf.Min(propACount + 1, 3); // 限制最大为3
                Debug.Log("拾取了一个 PropA，道具数量：" + propACount);
                Destroy(target); // 或改为 target.SetActive(false);
                return; // 只拾取一个道具
            }
            else if (target.CompareTag("PropB"))
            {
                // 拾取 PropB 道具（此处你可自行添加逻辑）
                Debug.Log("拾取了一个 PropB");
                Destroy(target); // 或改为 target.SetActive(false);
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
