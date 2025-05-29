using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] patrolPoints;
    [Tooltip("敌人巡逻时的速度")]
    public float patrolSpeed = 2f;
    private int currentPatrolIndex = 0;

    [Header("追击设置")]
    [Tooltip("侦测主角的距离")]
    public float detectionRange = 10f;
    [Tooltip("侦测主角的角度（度）")]
    public float detectionAngle = 60f; // ✅ 新增可调节检测角度
    [Tooltip("敌人追击主角时的速度")]
    public float chaseSpeed = 4f;
    [Tooltip("丢失目标后返回巡逻的等待时间")]
    public float loseTargetTime = 3f;
    private float chaseTimer = 0f;

    [Header("目标与动画")]
    public Transform player;
    public Transform playerModel;
    public Animator enemyAnimator;
    public Animator playerAnimator;
    public ThirdPersonController Controller;

    [Header("攻击设置")]
    [Tooltip("抓住玩家的距离")]
    public float catchDistance = 1.5f;
    [Tooltip("黑屏UI组件")]
    public Image blackScreen;
    [Tooltip("黑屏淡入淡出时间")]
    public float blackFadeDuration = 1f;
    [Tooltip("黑屏保持时间")]
    public float blackStayDuration = 3f;
    [Tooltip("玩家重生点")]
    public Transform playerRespawnPoint;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private int state = 0; // 0:巡逻, 1:追击, 2:攻击

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = 10f;
        agent.angularSpeed = 360f;

        startPosition = transform.position;
        enemyAnimator.applyRootMotion = false;

        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = GetClosestPatrolPointIndex();
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case 0:
                Patrol();
                if (IsPlayerDetected(distanceToPlayer))
                {
                    state = 1;
                    enemyAnimator.SetBool("isWalking", false);
                    enemyAnimator.SetBool("isRunning", true);
                }
                break;

            case 1:
                ChasePlayer(distanceToPlayer);
                break;

            case 2:
                break; // 攻击状态由协程控制
        }

        RotateTowardsMovement();
    }

    // 检测玩家是否在视野内（距离+角度）
    bool IsPlayerDetected(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // 角度检测
        if (angleToPlayer > detectionAngle / 2f) return false;

        // 射线检测（防止穿墙）
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
        {
            if (!hit.transform.CompareTag("Player")) return false;
        }

        return true;
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        enemyAnimator.SetBool("isWalking", true);
        enemyAnimator.SetBool("isRunning", false);
    }

    void ChasePlayer(float distance)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (distance <= catchDistance)
        {
            state = 2;
            agent.isStopped = true;
            enemyAnimator.SetTrigger("Attack");
            playerAnimator.applyRootMotion = true;
            if (playerAnimator != null) playerAnimator.SetTrigger("Caught");
            StartCoroutine(HandlePlayerCaught()); // 触发捕获处理
        }
        else if (!IsPlayerDetected(distance))
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= loseTargetTime)
            {
                ReturnToPatrol();
            }
        }
        else
        {
            chaseTimer = 0f;
        }
    }

    void ReturnToPatrol()
    {
        chaseTimer = 0f;
        state = 0;
        agent.isStopped = false;
        currentPatrolIndex = GetClosestPatrolPointIndex();
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        enemyAnimator.SetBool("isRunning", false);
        enemyAnimator.SetBool("isWalking", true);
    }

    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // ✅ 修改后的协程：提前触发Rebirth动画
    IEnumerator HandlePlayerCaught()
    {
        yield return new WaitForSeconds(1f); // 等待攻击动画播放

        // 黑屏淡入
        float t = 0f;
        Color originalColor = blackScreen.color;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, 1f, t / blackFadeDuration));
            yield return null;
        }
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        // 重置位置（此时屏幕全黑）
        player.position = playerRespawnPoint.position;
        transform.position = startPosition;
        agent.Warp(startPosition);
        Controller.isCrouching = false;

        // 关键修改：提前触发复活动画！
        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetTrigger("Rebirth"); // 黑屏期间触发
        }

        // 重置玩家姿势
        playerModel.localPosition = Vector3.zero;
        playerModel.localRotation = Quaternion.identity;
        playerAnimator.SetBool("IsCrouching", false);

        // 黑屏保持
        yield return new WaitForSeconds(blackStayDuration);

        // 恢复敌人巡逻
        agent.isStopped = false;
        state = 0;
        currentPatrolIndex = GetClosestPatrolPointIndex();
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        enemyAnimator.SetBool("isRunning", false);
        enemyAnimator.SetBool("isWalking", true);

        // 黑屏淡出（此时Rebirth动画已在播放）
        t = 0f;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t / blackFadeDuration));
            yield return null;
        }
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    int GetClosestPatrolPointIndex()
    {
        int closestIndex = 0;
        float closestDistance = Vector3.Distance(transform.position, patrolPoints[0].position);
        for (int i = 1; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    // 可视化检测范围（编辑器调试用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 leftDir = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
    }
}
