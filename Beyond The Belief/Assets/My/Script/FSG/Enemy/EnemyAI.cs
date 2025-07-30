using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyLevel
{
    Level1,
    Level2
}

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

    [Header("技能命中判定")]
    public EnemyLevel enemyLevel = EnemyLevel.Level1;
    public string deathTrigger = "Die";
    public float deathFallDelay = 1f;
    public float deathFallDistance = 3f;
    public float deathFallDuration = 1f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private int state = 0; // 0:巡逻/待机, 1:追击, 2:攻击
    private bool isDead = false;

    // 新增：黑屏期间暂停AI
    private bool isInBlackScreen = false;
    [Header("特效设置")]
    public GameObject catchEffect; // 进入抓捕范围时需要关闭的特效

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = 10f;
        agent.angularSpeed = 360f;

        startPosition = transform.position;
        startRotation = transform.rotation;
        enemyAnimator.applyRootMotion = false;

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
    }

    void Update()
    {
        // 黑屏中或敌人死亡时，不执行任何检测和追击
        if (isDead || isInBlackScreen) return;

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
                    agent.SetDestination(transform.position);
                    enemyAnimator.SetBool("isWalking", false);
                    enemyAnimator.SetBool("isRunning", false);
                    enemyAnimator.SetTrigger("Idle");
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

        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
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
        enemyAnimator.ResetTrigger("Idle");
    }

    void ChasePlayer(float distance)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (distance <= catchDistance)
        {
            if (Controller.isDead) return;
            Controller.isDead = true;

            if (catchEffect != null)
            {
                catchEffect.SetActive(false);
            }

            state = 2;
            agent.isStopped = true;
            enemyAnimator.SetTrigger("Attack");
            enemyAnimator.SetBool("isRunning", false);

            if (playerAnimator != null)
            {
                playerAnimator.applyRootMotion = true;
                Controller.isDead = true;
                playerAnimator.SetTrigger("Caught");
                playerAnimator.speed = 1;
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
            enemyAnimator.SetTrigger("Idle");
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
        isInBlackScreen = true; // 黑屏开始，暂停AI

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

        if (SkillPointManager.Instance != null && SkillPointRecord.Instance != null)
        {
            int savedPoints = SkillPointRecord.Instance.GetRememberedPoints();
            SkillPointManager.Instance.SetSkillPoints(savedPoints);
            Debug.Log($"死亡，技能点恢复为保存值：{savedPoints}");
        }

        player.position = playerRespawnPoint.position;
        transform.position = startPosition;
        transform.rotation = startRotation;
        agent.Warp(startPosition);
        Controller.isCrouching = false;
        Controller.isDead = false;

        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetTrigger("Rebirth");
        }

        playerModel.localPosition = Vector3.zero;
        playerModel.localRotation = Quaternion.identity;
        playerAnimator.SetBool("IsCrouching", false);

        var zones = FindObjectsOfType<ForceDetectionZone>();
        foreach (var zone in zones)
        {
            zone.ResetTrigger();
        }

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

        Controller.isDead = false;
        isInBlackScreen = false; // 黑屏结束，恢复AI
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

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (!other.CompareTag("SkillTrigger")) return;
        Debug.Log("识别技能trigger");
        var itemManager = FindObjectOfType<ItemPickupManager>();
        if (itemManager == null) return;

        int count = itemManager.propACount;

        if ((enemyLevel == EnemyLevel.Level1 && count < 3) || count >= 3)
        {
            DieFromSkill();
        }
    }

    void DieFromSkill()
    {
        isDead = true;
        enemyAnimator.SetTrigger(deathTrigger);
        agent.isStopped = true;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        agent.enabled = false;
        yield return new WaitForSeconds(deathFallDelay);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * deathFallDistance;
        float t = 0f;

        while (t < deathFallDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / deathFallDuration);
            yield return null;
        }

        float delayAfterFall = 1f;
        yield return new WaitForSeconds(delayAfterFall);
        //Debug.Log("死去的敌人该隐藏了");
        gameObject.SetActive(false);
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
