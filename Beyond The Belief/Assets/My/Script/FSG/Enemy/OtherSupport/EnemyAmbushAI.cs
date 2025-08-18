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
    public float slowPatrolSpeed = 1f; // 在减速区巡逻速度
    public float slowChaseSpeed = 2f;  // 在减速区追击速度
    private bool isInSlowZone = false; // 是否在减速区

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
    public bool isAmbushActive = false; // 初始为false，表示未激活
    public string wakeTrigger = "Wake"; // wake动画触发器
    public Transform patrolAreaCenter; // 巡逻区域中心点
    public float patrolAreaRadius = 5f; // 巡逻区域半径
    public float ambushDelay = 2f; // 触发后等待秒数
    private Vector3 ambushStartPosition;
    private Quaternion ambushStartRotation;
    public GameObject ambushTriggerBox;

    [Header("音效设置")]
    public AudioSource audioSource;      // 用于播放声音
    public AudioClip[] enemySounds;      // 敌人叫声集合
    public float minSoundInterval = 3f;  // 最短间隔
    public float maxSoundInterval = 7f;  // 最长间隔
    private float soundTimer = 0f;       // 计时器
    private float nextSoundDelay = 0f;   // 下一次播放延迟
    public AudioClip attackSound;          // 抓到玩家时播放的音效
    public float attackVolume = 1f;    // 默认音量


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = 10f;
        agent.angularSpeed = 360f;
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        enemyAnimator.applyRootMotion = false;

        if (enemyType == EnemyType.Ambush)
        {
            // 埋伏状态：完全静止，动画进入Idle或空状态
            agent.enabled = false;
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.ResetTrigger("Idle");

            ambushStartPosition = transform.localPosition;
            //Debug.Log("记录坐标" + transform.localPosition + ambushStartPosition);
            ambushStartRotation = transform.localRotation;
            return;
        }

        // 普通敌人的初始化
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
        nextSoundDelay = Random.Range(minSoundInterval, maxSoundInterval);

    }

    void Update()
    {
       
        if (enemyType == EnemyType.Ambush && Controller.isDead && state != 0)
        {
            StartCoroutine(ResetAmbushAfterDelay(2f));
            isAmbushActive = false;
            return;
        }
        if (isDead || isInBlackScreen) return;

        PlayRandomSoundIfNeeded();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);


        switch (state)
        {
            case 0:
                if (canPatrol) Patrol();
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
        /*agent.speed = isInSlowZone ? slowPatrolSpeed : patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        enemyAnimator.SetBool("isWalking", true);
        enemyAnimator.SetBool("isRunning", false);
        enemyAnimator.ResetTrigger("Idle");*/
    }

    void ChasePlayer(float distance)
    {
        agent.speed = isInSlowZone ? slowChaseSpeed : chaseSpeed;
        agent.SetDestination(player.position);

        if (distance <= catchDistance)
        {
            if (Controller.isDead) return;
            Controller.isDead = true;

            if (catchEffect != null) catchEffect.SetActive(false);

            // 播放攻击音效
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound, attackVolume);
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
                Controller.canMove = false;
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
        agent.isStopped = false;

        if (enemyType == EnemyType.Ambush)
        {
            StartCoroutine(RandomPatrolInArea());
            return;
        }

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

    IEnumerator RandomPatrolInArea()
    {
        while (state == 0) // 巡逻状态
        {
            Vector3 randomPoint = patrolAreaCenter.position + Random.insideUnitSphere * patrolAreaRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolAreaRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            enemyAnimator.SetBool("isWalking", true);
            enemyAnimator.SetBool("isRunning", false);
            yield return new WaitForSeconds(Random.Range(3f, 6f)); // 随机停留时间
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
        isInBlackScreen = true;

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
        foreach (var zone in zones) zone.ResetTrigger();

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
        Controller.canMove = true;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t / blackFadeDuration));
            yield return null;
        }
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Controller.isDead = false;
        isInBlackScreen = false;
        Controller.canMove = true;

        if (ambushTriggerBox != null)
        {
            ambushTriggerBox.SetActive(true);
        }




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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Marsh")) // 进入减速区
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
        if (other.CompareTag("Marsh")) // 离开减速区
        {
            isInSlowZone = false;
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

        yield return new WaitForSeconds(1f);
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

    public void ActivateAmbush()
    {
        if (enemyType != EnemyType.Ambush || isAmbushActive) return;

        isAmbushActive = true;
        StartCoroutine(DelayBeforeWake());
    }

    IEnumerator DelayBeforeWake()
    {
        // 先等待指定秒数
        yield return new WaitForSeconds(ambushDelay);

        // 激活AI并播放wake动画
        agent.enabled = true;
        agent.isStopped = true; // 等待wake动画播放
        enemyAnimator.SetTrigger(wakeTrigger);

        // 等待wake动画播放完成再追击
        StartCoroutine(WaitWakeAndChase());
    }



    IEnumerator WaitWakeAndChase()
    {
        // 这里假设wake动画长度为1.5秒，可以改成用动画事件触发
        yield return new WaitForSeconds(1.5f);
        state = 1; // 切换到追击模式
        agent.isStopped = false;
        enemyAnimator.SetBool("isRunning", true);
    }

    private bool isResettingAmbush = false;

    IEnumerator ResetAmbushAfterDelay(float delay)
    {
        if (isResettingAmbush) yield break; // 防止重复执行
        isResettingAmbush = true;

        agent.isStopped = true;
        yield return new WaitForSeconds(delay);

        Debug.Log("Ambush重新设置");
        // 回到埋伏点
        //agent.Warp(ambushStartPosition);
        //transform.rotation = ambushStartRotation;
        transform.localPosition = ambushStartPosition;
        Debug.Log("重置坐标" + transform.localPosition + ambushStartPosition);
        transform.localRotation = ambushStartRotation;


        // 停止动画，回到埋伏Idle状态
        enemyAnimator.SetBool("isWalking", false);
        enemyAnimator.SetBool("isRunning", false);
        //enemyAnimator.ResetTrigger("Ambush");
        //enemyAnimator.Play("BelowTheGround", 0, 0f); // 直接播放Idle动画（根据你的动画名称调整）
        enemyAnimator.SetTrigger("CanAmbush");

       


        // 重新禁用NavMeshAgent
        agent.enabled = false;

        // 重新允许激活wake trigger
        isAmbushActive = false;

        // 回到待机状态
        state = 0;

        isResettingAmbush = false;
    }

    void PlayRandomSoundIfNeeded()
    {
        // 只在巡逻或追击时才发声
        if (state != 0 && state != 1) return;
        if (enemySounds == null || enemySounds.Length == 0 || audioSource == null) return;

        soundTimer += Time.deltaTime;

        if (soundTimer >= nextSoundDelay)
        {
            // 随机选一个音效
            int index = Random.Range(0, enemySounds.Length);
            audioSource.PlayOneShot(enemySounds[index]);

            // 重置计时器 & 随机下次延迟
            soundTimer = 0f;
            nextSoundDelay = Random.Range(minSoundInterval, maxSoundInterval);
        }

    }

}
