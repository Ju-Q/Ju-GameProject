using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossMiniAmbushAI : MonoBehaviour
{
    [Header("埋伏设置")]
    public Transform patrolAreaCenter;
    public float patrolAreaRadius = 5f;
    public float ambushDelay = 2f;
    public float wakeUpDuration = 3f;
    public string wakeTrigger = "Wake";
    public GameObject ambushTriggerBox;

    [Header("追击设置")]
    public Transform player;
    public NavMeshAgent agent;
    public float chaseSpeed = 4f;
    public float catchDistance = 1.5f;

    [Header("动画设置")]
    public Animator enemyAnimator;

    [Header("特效设置")]
    public GameObject catchEffect;

    [Header("血量系统")]
    public PlayerHealth playerHealth; // 你的血量脚本（内部有黑屏）

    [Header("死亡设置")]
    public float deathSinkDuration = 2f; // 下降到地底的持续时间
    public string deathTrigger = "Die"; // 死亡动画触发器名称

    private Vector3 ambushStartPosition;
    private Quaternion ambushStartRotation;
    private bool isWakingUp = false;
    private bool isAmbushActive = false;
    private bool canCatchPlayer = true;
    private bool isAlive = true; // 添加存活状态

    void Start()
    {
        ambushStartPosition = transform.position;
        ambushStartRotation = transform.rotation;

        if (agent != null) agent.enabled = false;

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.SetTrigger("Idle");
        }

        canCatchPlayer = true;
        isAlive = true; // 初始化为存活状态
    }

    void Update()
    {
        if (!isAlive) return; // 如果死亡，不执行任何更新逻辑

        if (!isAmbushActive || isWakingUp || player == null || !canCatchPlayer)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        RotateTowardsPlayer();

        if (agent != null && agent.enabled)
            agent.SetDestination(player.position);
    }

    // 添加触发器检测方法
    void OnTriggerEnter(Collider other)
    {
        if (!isAlive) return; // 如果已经死亡，不再处理触发

        // 检测是否碰到技能触发器
        if (other.CompareTag("SkillTrigger"))
        {
            Die();
        }
    }

    // 敌人死亡方法
    public void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        canCatchPlayer = false;

        // 停止所有协程
        StopAllCoroutines();

        // 停止导航
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 播放死亡动画
        if (enemyAnimator != null)
        {
            // 重置所有可能的动画状态
            enemyAnimator.ResetTrigger("Attack");
            enemyAnimator.ResetTrigger("Wake");
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);

            // 触发死亡动画
            enemyAnimator.SetTrigger(deathTrigger);
        }

        // 开始下降到地底的协程
        StartCoroutine(DeathSinkRoutine());
    }

    // 死亡后下降到地底的协程
    private IEnumerator DeathSinkRoutine()
    {
        // 等待死亡动画播放一段时间（可选）
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition - Vector3.up * 2f; // 下降2个单位到地底

        // 逐渐下降到地底
        while (timer < deathSinkDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / deathSinkDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        // 完全下降到地底后，可以销毁敌人或设置为非激活
        gameObject.SetActive(false);

        // 或者如果你想要重置而不是销毁，可以调用：
        // ResetAmbush();
    }

    public void ActivateAmbush()
    {
        if (isAmbushActive || !isAlive) return;

        isAmbushActive = true;
        canCatchPlayer = false;
        StartCoroutine(AmbushRoutine());
    }

    private IEnumerator AmbushRoutine()
    {
        yield return new WaitForSeconds(ambushDelay);

        if (enemyAnimator != null)
            enemyAnimator.SetTrigger(wakeTrigger);

        isWakingUp = true;

        yield return new WaitForSeconds(wakeUpDuration);

        isWakingUp = false;
        canCatchPlayer = true;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
            agent.speed = chaseSpeed;
        }

        // ✅ 起身后直接切到跑步动画
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", true);
        }
    }

    private void CatchPlayer()
    {
        if (!isAlive) return;

        canCatchPlayer = false;

        // 播放攻击动画
        if (enemyAnimator != null)
        {
            enemyAnimator.ResetTrigger("Attack"); // 先清理上一次触发
            enemyAnimator.SetBool("isRunning", false);
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetTrigger("Attack");
        }

        if (catchEffect != null)
            catchEffect.SetActive(true);

        // 玩家死亡立即执行
        if (playerHealth != null)
            playerHealth.Die();

        // 敌人延迟回埋伏状态
        StartCoroutine(DelayedResetAmbush(1.0f)); // 根据攻击动画长度调整时间
    }

    private IEnumerator DelayedResetAmbush(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetAmbush();
    }

    private void ResetAmbush()
    {
        // 如果敌人已经死亡，不重置
        if (!isAlive) return;

        // 回埋伏点
        transform.position = ambushStartPosition;
        transform.rotation = ambushStartRotation;

        // 禁用 NavMeshAgent
        if (agent != null) agent.enabled = false;

        if (enemyAnimator != null)
        {
            // 清理攻击状态
            enemyAnimator.ResetTrigger("Attack");
            enemyAnimator.ResetTrigger("Wake");
            enemyAnimator.SetBool("isWalking", false);
            enemyAnimator.SetBool("isRunning", false);

            // 触发埋伏动画（地底/隐身状态）
            enemyAnimator.SetTrigger("CanAmbush");  // ✅ 用这个，而不是 Idle
        }

        if (ambushTriggerBox != null)
            ambushTriggerBox.SetActive(true);

        // 重置逻辑状态
        isAmbushActive = false;
        canCatchPlayer = true;
    }

    private void RotateTowardsPlayer()
    {
        if (player == null || !isAlive) return;
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
}