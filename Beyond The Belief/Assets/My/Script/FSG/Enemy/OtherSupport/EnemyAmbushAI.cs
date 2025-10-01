using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyLevel2
{
    Level1,
    Level2
}
public enum EnemyType
{
    Normal,
    Ambush
}

public class EnemyAmbushAI : MonoBehaviour
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

    [Header("减速区域速度设置")]
    public float slowPatrolSpeed = 1f;
    public float slowChaseSpeed = 2f;
    private bool isInSlowZone = false;

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

    private bool isInBlackScreen = false; // 黑屏暂停
    [Header("特效设置")]
    public GameObject catchEffect;

    [Header("敌人类型")]
    public EnemyType enemyType = EnemyType.Normal;

    [Header("埋伏模式设置")]
    public bool isAmbushActive = false;
    public string wakeTrigger = "Wake";
    public float wakeUpDuration = 3f; // 苏醒动画时长（播放wake动画的真实时长）
    private bool isWakingUp = false; // 是否正在苏醒（在此期间不允许位移）
    public Transform patrolAreaCenter;
    public float patrolAreaRadius = 5f;
    public float ambushDelay = 2f; // 触发后等待（比如玩家触发触发箱后等待多久开始wake）
    private Vector3 ambushStartPosition;
    private Quaternion ambushStartRotation;
    public GameObject ambushTriggerBox;
    private bool canCatchPlayer = true;

    [Header("音效设置")]
    public AudioSource audioSource;
    public AudioClip[] enemySounds;
    public float minSoundInterval = 3f;
    public float maxSoundInterval = 7f;
    private float soundTimer = 0f;
    private float nextSoundDelay = 0f;
    public AudioClip attackSound;
    public float attackVolume = 1f;

    private bool isResettingAmbush = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.acceleration = 10f;
            agent.angularSpeed = 360f;
        }

        // 使用世界坐标，便于 Warp/SetDestination 正确工作
        startPosition = transform.position;
        startRotation = transform.rotation;
        enemyAnimator.applyRootMotion = false;
        canCatchPlayer = true;

        if (enemyType == EnemyType.Ambush)
        {
            // 初始埋伏状态：禁用 agent，播放 idle
            if (agent != null) agent.enabled = false;

            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.ResetTrigger("Idle");
            }

            ambushStartPosition = transform.position;
            ambushStartRotation = transform.rotation;
            return;
        }

        // 普通敌人初始化（在 agent 可用时设置路径）
        if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = GetClosestPatrolPointIndex();
            if (agent != null && agent.enabled)
            {
                Vector3 sample;
                if (SampleNavMeshPosition(patrolPoints[currentPatrolIndex].position, out sample))
                    agent.Warp(sample);
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", true);
                enemyAnimator.ResetTrigger("Idle");
            }
        }
        else
        {
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.SetTrigger("Idle");
            }
        }
        nextSoundDelay = Random.Range(minSoundInterval, maxSoundInterval);
    }
    void Update()
    {
        if (isDead || isInBlackScreen) return;

        // 🔹 修改：埋伏敌人监听主角死亡，不再判断 state
        if (enemyType == EnemyType.Ambush && Controller != null && Controller.isDead)
        {
            if (!isResettingAmbush)
            {
                StartCoroutine(ResetAmbushAfterDelay(2f));
            }
            return;
        }

        PlayRandomSoundIfNeeded();

        if (player == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case 0:
                // 🔹 修改：埋伏敌人不自动追击，不做巡逻或检测
                if (enemyType == EnemyType.Ambush)
                {
                    // 维持当前 idle 状态
                    if (enemyAnimator != null)
                    {
                        enemyAnimator.SetBool("isWalking", false);
                        enemyAnimator.SetBool("isRunning", false);
                        enemyAnimator.ResetTrigger("Idle");
                    }
                    break;
                }

                if (canPatrol) Patrol();
                else
                {
                    if (agent != null && agent.enabled) agent.SetDestination(transform.position);
                    if (enemyAnimator != null)
                    {
                        enemyAnimator.SetBool("isWalking", false);
                        enemyAnimator.SetBool("isRunning", false);
                        enemyAnimator.SetTrigger("Idle");
                    }
                }

                if (IsPlayerDetected(distanceToPlayer))
                {
                    state = 1;
                    if (enemyAnimator != null)
                    {
                        enemyAnimator.SetBool("isWalking", false);
                        enemyAnimator.SetBool("isRunning", true);
                        enemyAnimator.ResetTrigger("Idle");
                    }
                }
                break;

            case 1:
                ChasePlayer(distanceToPlayer);
                break;

            case 2:
                // 攻击中 — 等待协程处理
                break;
        }

        RotateTowardsMovement();
    }

    bool IsPlayerDetected(float distanceToPlayer)
    {
        if (player == null) return false;
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
        // 如果 agent 不存在或未启用则不执行巡逻逻辑
        if (agent == null || !agent.enabled) return;

        // 防止除以零
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.SetDestination(transform.position);
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.SetTrigger("Idle");
            }
            return;
        }

        agent.speed = isInSlowZone ? slowPatrolSpeed : patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isWalking", true);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.ResetTrigger("Idle");
        }
    }

    void ChasePlayer(float distance)
    {
        // 如果正在苏醒，啥也不做（重要：不要调用 SetDestination）
        if (isWakingUp) return;

        // 如果 agent 不存在或未启用则不追击
        if (agent == null || !agent.enabled) return;

        agent.speed = isInSlowZone ? slowChaseSpeed : chaseSpeed;

        // 更新目的地（只有 agent 启用且不是苏醒期间才会执行）
        agent.SetDestination(player.position);

        // 抓捕逻辑
        if (distance <= catchDistance)
        {
            if (!canCatchPlayer) return;
            if (Controller != null && Controller.isDead) return;

            if (Controller != null) Controller.isDead = true;

            if (catchEffect != null) catchEffect.SetActive(false);

            if (audioSource != null && attackSound != null)
                audioSource.PlayOneShot(attackSound, attackVolume);

            state = 2;

            // 停止 agent 以触发攻击动画（agent 已启用）
            agent.isStopped = true;

            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Attack");
                enemyAnimator.SetBool("isRunning", false);
            }

            if (playerAnimator != null)
            {
                playerAnimator.applyRootMotion = true;
                if (Controller != null) Controller.isDead = true;
                playerAnimator.SetTrigger("Caught");
                if (Controller != null) Controller.canMove = false;
                playerAnimator.speed = 1;
            }

            StartCoroutine(HandlePlayerCaught());
        }
        else if (!IsPlayerDetected(distance))
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= loseTargetTime) ReturnToPatrol();
        }
        else chaseTimer = 0f;
    }

    void ReturnToPatrol()
    {
        chaseTimer = 0f;
        state = 0;

        if (agent == null || !agent.enabled)
        {
            // 若 agent 不可用，直接设置动画Idle
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.SetTrigger("Idle");
            }
            return;
        }

        agent.isStopped = false;

        if (enemyType == EnemyType.Ambush)
        {
            StartCoroutine(RandomPatrolInArea());
            return;
        }

        if (canPatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = GetClosestPatrolPointIndex();
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", true);
                enemyAnimator.ResetTrigger("Idle");
            }
        }
        else
        {
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.SetTrigger("Idle");
            }
        }
    }

    IEnumerator RandomPatrolInArea()
    {
        while (state == 0)
        {
            Vector3 randomPoint = patrolAreaCenter.position + Random.insideUnitSphere * patrolAreaRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolAreaRadius, NavMesh.AllAreas))
            {
                if (agent != null && agent.enabled) agent.SetDestination(hit.position);
            }
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", true);
                enemyAnimator.SetBool("isRunning", false);
            }
            yield return new WaitForSeconds(Random.Range(3f, 6f));
        }
    }

    void RotateTowardsMovement()
    {
        if (agent == null) return;
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    IEnumerator HandlePlayerCaught()
    {
        isInBlackScreen = true;

        // 等待短暂延迟，让抓捕动画播放一帧
        yield return new WaitForSeconds(0.1f);

        // 黑屏淡入
        float t = 0f;
        Color originalColor = blackScreen != null ? blackScreen.color : new Color(0, 0, 0, 0);
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            if (blackScreen != null)
                blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, 1f, t / blackFadeDuration));
            yield return null;
        }
        if (blackScreen != null) blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        // 重置玩家状态
        if (SkillPointManager.Instance != null && SkillPointRecord.Instance != null)
        {
            int savedPoints = SkillPointRecord.Instance.GetRememberedPoints();
            SkillPointManager.Instance.SetSkillPoints(savedPoints);
        }

        if (player != null && playerRespawnPoint != null)
            player.position = playerRespawnPoint.position;

        if (Controller != null)
        {
            Controller.isCrouching = false;
            Controller.isDead = false;
            Controller.canMove = true;
        }

        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetTrigger("Rebirth");
            playerAnimator.SetBool("IsCrouching", false);
        }

        if (playerModel != null)
        {
            playerModel.localPosition = Vector3.zero;
            playerModel.localRotation = Quaternion.identity;
        }

        // 🔹 重置敌人
        if (enemyType == EnemyType.Ambush)
        {
            // Warp 回埋伏点
            transform.position = ambushStartPosition;
            transform.rotation = ambushStartRotation;

            // 禁用 agent
            if (agent != null) agent.enabled = false;

            // 回到埋伏 Idle 动画
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", false);
                enemyAnimator.SetTrigger("CanAmbush");
            }

            // 状态重置
            state = 0;
            isAmbushActive = false;
            isResettingAmbush = false;
        }
        else
        {
            // 普通敌人回到巡逻
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
                ReturnToPatrol();
            }
        }

        // 重置黑屏停留时间
        yield return new WaitForSeconds(blackStayDuration);

        // 黑屏淡出
        t = 0f;
        if (blackScreen != null)
        {
            while (t < blackFadeDuration)
            {
                t += Time.deltaTime;
                blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t / blackFadeDuration));
                yield return null;
            }
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        isInBlackScreen = false;

        // 触发器重置
        if (ambushTriggerBox != null)
            ambushTriggerBox.SetActive(true);
    }


    int GetClosestPatrolPointIndex()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return 0;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Marsh"))
        {
            isInSlowZone = true;
            return;
        }

        if (isDead) return;
        if (!other.CompareTag("SkillTrigger")) return;

        var itemManager = FindObjectOfType<ItemPickupManager>();
        if (itemManager == null) return;

        int count = itemManager.propACount;
        if ((enemyLevel == EnemyLevel.Level1 && count < 3) || count >= 3)
        {
            DieFromSkill();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Marsh"))
        {
            isInSlowZone = false;
        }
    }

    void DieFromSkill()
    {
        isDead = true;
        if (enemyAnimator != null) enemyAnimator.SetTrigger(deathTrigger);
        if (agent != null) agent.isStopped = true;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        if (agent != null) agent.enabled = false;
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

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    public void TryForceDetection()
    {
        if (state == 0)
        {
            state = 1;
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("isWalking", false);
                enemyAnimator.SetBool("isRunning", true);
                enemyAnimator.ResetTrigger("Idle");
            }
        }
    }

    // 统一的埋伏激活入口：会先等待 ambushDelay，然后播放 wake 动画并在 wakeUpDuration 内保持不动，最后启用 agent 并开始追击
    public void ActivateAmbush()
    {
        if (enemyType != EnemyType.Ambush || isAmbushActive) return;

        isAmbushActive = true;
        canCatchPlayer = false;
        StartCoroutine(DelayBeforeWake());
    }

    IEnumerator DelayBeforeWake()
    {
        // 等待触发前的延迟（例如触发箱激活后有个延迟）
        yield return new WaitForSeconds(ambushDelay);

        // 播放唤醒动画（苏醒期间禁止位移）
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger(wakeTrigger);
        }

        // 保证在唤醒期间不会被 NavMeshAgent 控制位置：**禁用 agent**
        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }

        isWakingUp = true;

        // 等待苏醒动画时长（根据你的动画长度设置）
        yield return new WaitForSeconds(wakeUpDuration);

        // 苏醒结束：重新启用 agent，Warp 到当前 transform 以确保 agent 位于 NavMesh 上
        isWakingUp = false;
        canCatchPlayer = true;

        if (agent != null)
        {
            agent.enabled = true;

            // 优先 sample 一个 navmesh 点来 Warp，若找不到就直接 Warp 到 transform.position（可能无效）
            Vector3 sample;
            if (SampleNavMeshPosition(transform.position, out sample))
            {
                agent.Warp(sample);
                // 首次设置目的地开始追击
                agent.SetDestination(player != null ? player.position : transform.position);
            }
            else
            {
                agent.Warp(transform.position);
                if (player != null) agent.SetDestination(player.position);
            }

            agent.isStopped = false;
        }

        // 切换状态为追击
        state = 1;
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isRunning", true);
            enemyAnimator.SetBool("isWalking", false);
        }
    }

    // 埋伏重置：将敌人传回埋伏点并禁用 agent
    IEnumerator ResetAmbushAfterDelay(float delay)
    {
        if (isResettingAmbush) yield break;
        isResettingAmbush = true;

        if (agent != null && agent.enabled)
            agent.isStopped = true;

        yield return new WaitForSeconds(delay);

        // 回埋伏点（优先使用 world 坐标记录）
        if (ambushStartPosition != Vector3.zero)
        {
            transform.position = ambushStartPosition;
        }
        transform.rotation = ambushStartRotation;

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.SetTrigger("CanAmbush");
        }

        // 禁用 agent 回到埋伏
        if (agent != null) agent.enabled = false;

        isAmbushActive = false;
        state = 0;
        isResettingAmbush = false;
    }

    void PlayRandomSoundIfNeeded()
    {
        if (state != 0 && state != 1) return;
        if (enemySounds == null || enemySounds.Length == 0 || audioSource == null) return;

        soundTimer += Time.deltaTime;

        if (soundTimer >= nextSoundDelay)
        {
            int index = Random.Range(0, enemySounds.Length);
            audioSource.PlayOneShot(enemySounds[index]);

            soundTimer = 0f;
            nextSoundDelay = Random.Range(minSoundInterval, maxSoundInterval);
        }
    }

    // 帮助：寻找 NavMesh 上的最近点
    private bool SampleNavMeshPosition(Vector3 target, out Vector3 result)
    {
        NavMeshHit hit;
        float sampleRadius = 2.0f;
        if (NavMesh.SamplePosition(target, out hit, sampleRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = target;
        return false;
    }
}
