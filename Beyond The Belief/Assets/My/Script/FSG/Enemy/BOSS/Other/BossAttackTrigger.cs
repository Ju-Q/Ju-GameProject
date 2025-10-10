using UnityEngine;

public class BossAttackTrigger : MonoBehaviour
{
    [Header("触发器设置")]
    [Tooltip("触发后是否禁用触发器（一次性触发）")]
    public bool DisableAfterTrigger = true;

    [Tooltip("触发后延迟几秒开启Boss攻击")]
    public float DelayBeforeStart = 0f;

    [Header("Boss引用")]
    [Tooltip("自动获取或手动指定Boss的EnemyAttackController")]
    public EnemyAttackController bossAttackController;

    [Header("调试")]
    public bool enableDebugLogs = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // 如果未手动指定，尝试自动获取
        if (bossAttackController == null)
        {
            bossAttackController = FindObjectOfType<EnemyAttackController>();

            if (enableDebugLogs)
            {
                if (bossAttackController != null)
                    Debug.Log($"[BossTrigger] 自动找到Boss攻击控制器: {bossAttackController.gameObject.name}");
                else
                    Debug.LogWarning("[BossTrigger] 未找到EnemyAttackController，请手动指定");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 如果已经触发过，直接返回
        if (hasTriggered) return;

        // 检查是否是玩家（根据你的标签设置调整）
        if (other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log($"[BossTrigger] 玩家进入触发器: {other.gameObject.name}");

            TriggerBossAttack();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果已经触发过，直接返回
        if (hasTriggered) return;

        // 检查是否是玩家（2D版本）
        if (other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log($"[BossTrigger] 玩家进入触发器(2D): {other.gameObject.name}");

            TriggerBossAttack();
        }
    }

    private void TriggerBossAttack()
    {
        if (bossAttackController == null)
        {
            Debug.LogError("[BossTrigger] ❌ Boss攻击控制器未设置！");
            return;
        }

        hasTriggered = true;

        if (DelayBeforeStart > 0f)
        {
            // 延迟开启
            StartCoroutine(StartBossAttackWithDelay());
        }
        else
        {
            // 立即开启
            StartBossAttack();
        }

        // 如果需要一次性触发，禁用触发器
        if (DisableAfterTrigger)
        {
            Collider collider3D = GetComponent<Collider>();
            Collider2D collider2D = GetComponent<Collider2D>();

            if (collider3D != null) collider3D.enabled = false;
            if (collider2D != null) collider2D.enabled = false;

            if (enableDebugLogs)
                Debug.Log("[BossTrigger] 触发器已禁用");
        }
    }

    private System.Collections.IEnumerator StartBossAttackWithDelay()
    {
        if (enableDebugLogs)
            Debug.Log($"[BossTrigger] 等待 {DelayBeforeStart} 秒后开启Boss攻击...");

        yield return new WaitForSeconds(DelayBeforeStart);

        StartBossAttack();
    }

    private void StartBossAttack()
    {
        bossAttackController.CanStartAttack = true;

        if (enableDebugLogs)
            Debug.Log($"[BossTrigger] ✅ Boss攻击已开启！当前阶段: {bossAttackController.CurrentPhaseIndex}");
    }

    // 重置触发器状态（可用于Boss战重置）
    public void ResetTrigger()
    {
        hasTriggered = false;

        // 重新启用碰撞器
        Collider collider3D = GetComponent<Collider>();
        Collider2D collider2D = GetComponent<Collider2D>();

        if (collider3D != null) collider3D.enabled = true;
        if (collider2D != null) collider2D.enabled = true;

        if (enableDebugLogs)
            Debug.Log("[BossTrigger] 触发器已重置");
    }

    // 手动触发Boss攻击（可用于调试或其他触发方式）
    [ContextMenu("手动触发Boss攻击")]
    public void TriggerManually()
    {
        if (hasTriggered)
        {
            Debug.LogWarning("[BossTrigger] 触发器已经触发过了");
            return;
        }

        TriggerBossAttack();
    }

    // 调试信息
    private void OnDrawGizmos()
    {
        // 在Scene视图中显示触发器范围
        Gizmos.color = hasTriggered ? Color.green : new Color(1f, 0.5f, 0f, 0.5f);

        // 3D碰撞器
        Collider collider3D = GetComponent<Collider>();
        if (collider3D != null)
        {
            if (collider3D is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);
            }
            else if (collider3D is SphereCollider sphereCollider)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);
            }
        }

        // 2D碰撞器
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (collider2D is BoxCollider2D boxCollider2D)
            {
                Gizmos.DrawWireCube(Vector3.zero, boxCollider2D.size);
            }
            else if (collider2D is CircleCollider2D circleCollider2D)
            {
                Gizmos.DrawWireSphere(Vector3.zero, circleCollider2D.radius);
            }
        }

        // 显示触发器名称
        GUIStyle style = new GUIStyle();
        style.normal.textColor = hasTriggered ? Color.green : Color.white;
        style.alignment = TextAnchor.MiddleCenter;

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, "Boss Attack Trigger\n" + (hasTriggered ? "已触发" : "未触发"), style);
#endif
    }
}