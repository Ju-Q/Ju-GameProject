using UnityEngine;
using UnityEngine.AI; // 即使 Navigation 是 Obsolete，NavMeshAgent 还是可用的

public class NPCPatrol : MonoBehaviour
{
    public Transform pointA; // 巡逻起点
    public Transform pointB; // 巡逻终点
    public float stopDistance = 0.2f; // 到达目标的判定距离
    public float turnDuration = 1.0f; // 转身动画时长

    private NavMeshAgent agent;
    private Animator animator;
    private Transform currentTarget;
    private bool isTurning = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        currentTarget = pointA;
        GoToTarget();
        animator.applyRootMotion = false;

    }

    private void Update()
    {
        if (!isTurning)
        {
            // 走路状态
            animator.SetBool("isWalking", true);

            // 检查是否到达
            if (!agent.pathPending && agent.remainingDistance <= stopDistance)
            {
                StartCoroutine(TurnAndSwitchTarget());
            }
        }
    }

    private void GoToTarget()
    {
        if (agent != null && currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    private System.Collections.IEnumerator TurnAndSwitchTarget()
    {
        isTurning = true;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("turnRight"); // 播放转身动画

        yield return new WaitForSeconds(turnDuration); // 等转身动画播放完

        // 切换目标点
        currentTarget = (currentTarget == pointA) ? pointB : pointA;

        agent.isStopped = false;
        GoToTarget();

        isTurning = false;
    }
}
