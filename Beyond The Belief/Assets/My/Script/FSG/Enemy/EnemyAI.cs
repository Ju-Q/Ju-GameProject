using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("巡逻设置")]
    public bool canPatrol = true;
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    private int currentPatrolIndex = 0;

    [Header("追击设置")]
    public float detectionRange = 10f;
    public float detectionAngle = 60f;
    public float chaseSpeed = 4f;
    public float loseTargetTime = 3f;
    private float chaseTimer = 0f;

    [Header("目标与动画")]
    public Transform player;
    public Transform playerModel;
    public Animator enemyAnimator;
    public Animator playerAnimator;
    public ThirdPersonController Controller;

    [Header("攻击设置")]
    public float catchDistance = 1.5f;
    public Image blackScreen;
    public float blackFadeDuration = 1f;
    public float blackStayDuration = 3f;
    public Transform playerRespawnPoint;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private int state = 0; // 0:巡逻/待机, 1:追击, 2:攻击

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = 10f;
        agent.angularSpeed = 360f;

        startPosition = transform.position;
        enemyAnimator.applyRootMotion = false;

        if (canPatrol && patrolPoints.Length > 0)
        {
            currentPatrolIndex = GetClosestPatrolPointIndex();
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            enemyAnimator.SetBool("isWalking", true);
            enemyAnimator.ResetTrigger("Idle"); // 取消Idle触发
        }
        else
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.SetTrigger("Idle"); // ✅ 非巡逻时播放Idle动画
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case 0:
                if (canPatrol)
                {
                    Patrol();
                }
                else
                {
                    agent.SetDestination(transform.position); // 停止移动
                    enemyAnimator.SetBool("isWalking", false);
                    enemyAnimator.SetBool("isRunning", false);
                    enemyAnimator.SetTrigger("Idle"); // ✅ 非巡逻状态保持Idle动画
                }

                if (IsPlayerDetected(distanceToPlayer))
                {
                    state = 1;
                    enemyAnimator.SetBool("isWalking", false);
                    enemyAnimator.SetBool("isRunning", true);
                    enemyAnimator.ResetTrigger("Idle");
                }
                break;

            case 1:
                ChasePlayer(distanceToPlayer);
                break;

            case 2:
                break;
        }

        RotateTowardsMovement();
    }

    bool IsPlayerDetected(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > detectionAngle / 2f) return false;

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
        enemyAnimator.ResetTrigger("Idle"); // ✅ 清除Idle状态
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

            if (playerAnimator != null)
            {
                playerAnimator.applyRootMotion = true;
                playerAnimator.SetTrigger("Caught");
            }

            StartCoroutine(HandlePlayerCaught());
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

        if (canPatrol && patrolPoints.Length > 0)
        {
            currentPatrolIndex = GetClosestPatrolPointIndex();
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            enemyAnimator.SetBool("isWalking", true);
            enemyAnimator.ResetTrigger("Idle");
        }
        else
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.SetTrigger("Idle"); // ✅ 非巡逻恢复Idle动画
        }
    }

    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    IEnumerator HandlePlayerCaught()
    {
        yield return new WaitForSeconds(1f);

        float t = 0f;
        Color originalColor = blackScreen.color;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, 1f, t / blackFadeDuration));
            yield return null;
        }

        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        player.position = playerRespawnPoint.position;
        transform.position = startPosition;
        agent.Warp(startPosition);
        Controller.isCrouching = false;

        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetTrigger("Rebirth");
        }

        playerModel.localPosition = Vector3.zero;
        playerModel.localRotation = Quaternion.identity;
        playerAnimator.SetBool("IsCrouching", false);

        yield return new WaitForSeconds(blackStayDuration);

        agent.isStopped = false;
        state = 0;

        if (canPatrol && patrolPoints.Length > 0)
        {
            currentPatrolIndex = GetClosestPatrolPointIndex();
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            enemyAnimator.SetBool("isWalking", true);
            enemyAnimator.ResetTrigger("Idle");
        }
        else
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.SetTrigger("Idle");
        }

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

    public void TryForceDetection()
    {
        if (state == 0)
        {
            state = 1;
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", true);
            enemyAnimator.ResetTrigger("Idle");
        }
    }
}
