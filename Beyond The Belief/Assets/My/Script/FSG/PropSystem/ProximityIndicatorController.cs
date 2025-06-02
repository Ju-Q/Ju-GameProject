using UnityEngine;

/// <summary>
/// 近距离交互提示控制器：检测玩家周围的可交互物体，并显示最近的交互提示
/// </summary>
public class ProximityIndicatorController : MonoBehaviour
{
    [Header("检测配置")]
    public float detectionRadius = 3f;          // 检测半径
    public LayerMask interactableLayer;         // 指定可交互物体所在的Layer（避免检测无关对象）

    private InteractableIndicator currentTarget; // 当前最近的交互目标

    private void Update()
    {
        // 1. 球形检测：获取半径内所有在指定Layer上的碰撞体
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);

        // 2. 寻找最近的交互目标
        InteractableIndicator closest = null;
        float minDistance = Mathf.Infinity;     // 初始设为无限大，便于比较

        foreach (var hit in hits)
        {
            // 尝试从碰撞体获取InteractableIndicator组件
            InteractableIndicator indicator = hit.GetComponent<InteractableIndicator>();
            if (indicator != null)
            {
                // 计算与玩家的距离
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    closest = indicator;        // 更新最近目标
                    minDistance = dist;         // 更新最小距离
                }
            }
        }

        // 3. 切换提示状态：如果最近目标发生变化
        if (closest != currentTarget)
        {
            // 隐藏旧目标的提示
            if (currentTarget != null)
                currentTarget.HideIndicator();

            // 更新当前目标
            currentTarget = closest;

            // 显示新目标的提示
            if (currentTarget != null)
                currentTarget.ShowIndicator();
        }
    }

    /// <summary>
    /// 在Unity编辑器中绘制检测范围的Gizmo（方便调试）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}