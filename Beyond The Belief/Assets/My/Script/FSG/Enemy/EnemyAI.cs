using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    private int currentPatrolIndex = 0;

    [Header("追击设置")]
    public float detectionRange = 10f;
    public float chaseSpeed = 4f;
    public float loseTargetTime = 3f;
    private float chaseTimer = 0f;

    [Header("目标与动画")]
    public Transform player;
    public Transform playerModel;    // 主角 Transform
    public Animator enemyAnimator;          // 敌人 Animator 拖入
    public Animator playerAnimator;         // 主角 Animator 拖入

    [Header("攻击设置")]
    public float catchDistance = 1.5f;       // 抓捕距离
    public Image blackScreen;                // UI 黑屏图像，类型为 Image
    public float blackFadeDuration = 1f;
    public float blackStayDuration = 3f;
    public Transform playerRespawnPoint;     // 主角重生点

    private NavMeshAgent agent;
    private Vector3 startPosition;           // 敌人起始位置
    private int state = 0;                   // 0: 巡逻，1: 追击，2: 攻击

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case 0: // 巡逻
                Patrol();
                if (distanceToPlayer < detectionRange)
                {
                    state = 1;
                    enemyAnimator.SetBool("isWalking", false);
                    enemyAnimator.SetBool("isRunning", true);
                }
                break;

            case 1: // 追击
                ChasePlayer(distanceToPlayer);
                break;

            case 2: // 攻击状态，不移动
                break;
        }

        RotateTowardsMovement();
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        // 到达当前巡逻点后前往下一个
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        // 设置动画
        enemyAnimator.SetBool("isWalking", true);
        enemyAnimator.SetBool("isRunning", false);
    }

    void ChasePlayer(float distance)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (distance <= catchDistance)
        {
            // 进入攻击状态
            state = 2;
            agent.isStopped = true;
            enemyAnimator.SetTrigger("Attack");

            // 主角被抓动画
            if (playerAnimator != null)
            {
                playerAnimator.applyRootMotion = true;
                playerAnimator.SetTrigger("Caught");
            }

            StartCoroutine(HandlePlayerCaught());
        }
        else if (distance > detectionRange)
        {
            // 如果距离太远，开始计时丢失
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= loseTargetTime)
            {
                chaseTimer = 0f;
                state = 0;
                agent.isStopped = false;

                // 找到离当前位置最近的巡逻点
                currentPatrolIndex = GetClosestPatrolPointIndex();
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);

                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.SetBool("isWalking", true);
            }
        }
        else
        {
            chaseTimer = 0f;
        }
    }

    // 转向当前移动方向
    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // 处理主角被抓后的黑屏逻辑
    IEnumerator HandlePlayerCaught()
    {
        yield return new WaitForSeconds(1f); // 等待攻击动画完成

        // 黑屏淡入
        float t = 0f;
        Color originalColor = blackScreen.color;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, 1f, t / blackFadeDuration));
            yield return null;
        }

        // 停留黑屏
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        yield return new WaitForSeconds(blackStayDuration);

        // 重置位置
        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetTrigger("Rebirth");
        }
        player.position = playerRespawnPoint.position;

        transform.position = startPosition;
        agent.Warp(startPosition); // 强制 NavMesh 重置
        agent.isStopped = false;
        state = 0;
        playerModel.localPosition = Vector3.zero;
        playerModel.localRotation = Quaternion.identity;
        playerAnimator.SetBool("IsCrouching", false);

        currentPatrolIndex = GetClosestPatrolPointIndex(); // 回到最近巡逻点
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        enemyAnimator.SetBool("isRunning", false);
        enemyAnimator.SetBool("isWalking", true);

        // 黑屏淡出
        t = 0f;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t / blackFadeDuration));
            yield return null;
        }
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    // 查找最近巡逻点索引
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
}
