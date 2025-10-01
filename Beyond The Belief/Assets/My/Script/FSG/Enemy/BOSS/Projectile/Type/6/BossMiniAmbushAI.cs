using UnityEngine;
using UnityEngine.AI;
using System.Collections;   // 🔹 必须有这一行，支持 IEnumerator

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

    private Vector3 ambushStartPosition;
    private Quaternion ambushStartRotation;
    private bool isWakingUp = false;
    private bool isAmbushActive = false;
    private bool canCatchPlayer = true;

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
    }

    void Update()
    {
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

    public void ActivateAmbush()
    {
        if (isAmbushActive) return;

        isAmbushActive = true;
        canCatchPlayer = false;
        StartCoroutine(AmbushRoutine());
    }

    private System.Collections.IEnumerator AmbushRoutine()
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



    private System.Collections.IEnumerator DelayedKillAndReset(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 玩家死亡（PlayerHealth自己处理黑屏）
        if (playerHealth != null)
            playerHealth.Die();

        // 重置敌人
        ResetAmbush();
    }

    private void ResetAmbush()
    {
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
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
}
