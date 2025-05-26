using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("巡逻点设置")]
    public Transform[] patrolPoints; // 巡逻路径点
    private int currentPointIndex = 0;
    private NavMeshAgent agent;

    [Header("主角监测设置")]
    public Transform player;              // 主角（拖入）
    public float viewAngle = 60f;         // 视野角度
    public float viewDistance = 10f;      // 视野距离
    public LayerMask obstacleMask;        // 障碍物层（如墙）

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPoint();
    }

    void Update()
    {
        // ========== 巡逻逻辑 ==========
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            GoToNextPoint();
        }

        // ========== 主角监测逻辑 ==========
        if (CanSeePlayer())
        {
            Debug.Log("敌人发现主角！");
            // 👉 这里你可以改为切换动画或开始追击主角
        }
    }

    // 巡逻移动到下一个路径点
    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    // 判断是否能看到主角
    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float distanceToPlayer = dirToPlayer.magnitude;

        if (distanceToPlayer > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angle < viewAngle / 2f)
        {
            // 射线检测是否有遮挡
            if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer.normalized, distanceToPlayer, obstacleMask))
            {
                return true;
            }
        }

        return false;
    }

    // 编辑器中显示视野范围
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftDir = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * viewDistance);
    }
}
