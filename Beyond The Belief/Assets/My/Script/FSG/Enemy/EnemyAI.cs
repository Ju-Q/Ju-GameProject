using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Ѳ������")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    private int currentPatrolIndex = 0;

    [Header("׷������")]
    public float detectionRange = 10f;
    public float chaseSpeed = 4f;
    public float loseTargetTime = 3f;
    private float chaseTimer = 0f;

    [Header("Ŀ���붯��")]
    public Transform player;
    public Transform playerModel;    // ���� Transform
    public Animator enemyAnimator;          // ���� Animator ����
    public Animator playerAnimator;         // ���� Animator ����

    [Header("��������")]
    public float catchDistance = 1.5f;       // ץ������
    public Image blackScreen;                // UI ����ͼ������Ϊ Image
    public float blackFadeDuration = 1f;
    public float blackStayDuration = 3f;
    public Transform playerRespawnPoint;     // ����������

    private NavMeshAgent agent;
    private Vector3 startPosition;           // ������ʼλ��
    private int state = 0;                   // 0: Ѳ�ߣ�1: ׷����2: ����

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
            case 0: // Ѳ��
                Patrol();
                if (distanceToPlayer < detectionRange)
                {
                    state = 1;
                    enemyAnimator.SetBool("isWalking", false);
                    enemyAnimator.SetBool("isRunning", true);
                }
                break;

            case 1: // ׷��
                ChasePlayer(distanceToPlayer);
                break;

            case 2: // ����״̬�����ƶ�
                break;
        }

        RotateTowardsMovement();
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        // ���ﵱǰѲ�ߵ��ǰ����һ��
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        // ���ö���
        enemyAnimator.SetBool("isWalking", true);
        enemyAnimator.SetBool("isRunning", false);
    }

    void ChasePlayer(float distance)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (distance <= catchDistance)
        {
            // ���빥��״̬
            state = 2;
            agent.isStopped = true;
            enemyAnimator.SetTrigger("Attack");

            // ���Ǳ�ץ����
            if (playerAnimator != null)
            {
                playerAnimator.applyRootMotion = true;
                playerAnimator.SetTrigger("Caught");
            }

            StartCoroutine(HandlePlayerCaught());
        }
        else if (distance > detectionRange)
        {
            // �������̫Զ����ʼ��ʱ��ʧ
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= loseTargetTime)
            {
                chaseTimer = 0f;
                state = 0;
                agent.isStopped = false;

                // �ҵ��뵱ǰλ�������Ѳ�ߵ�
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

    // ת��ǰ�ƶ�����
    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // �������Ǳ�ץ��ĺ����߼�
    IEnumerator HandlePlayerCaught()
    {
        yield return new WaitForSeconds(1f); // �ȴ������������

        // ��������
        float t = 0f;
        Color originalColor = blackScreen.color;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, 1f, t / blackFadeDuration));
            yield return null;
        }

        // ͣ������
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        yield return new WaitForSeconds(blackStayDuration);

        // ����λ��
        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
            playerAnimator.SetTrigger("Rebirth");
        }
        player.position = playerRespawnPoint.position;

        transform.position = startPosition;
        agent.Warp(startPosition); // ǿ�� NavMesh ����
        agent.isStopped = false;
        state = 0;
        playerModel.localPosition = Vector3.zero;
        playerModel.localRotation = Quaternion.identity;
        playerAnimator.SetBool("IsCrouching", false);

        currentPatrolIndex = GetClosestPatrolPointIndex(); // �ص����Ѳ�ߵ�
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        enemyAnimator.SetBool("isRunning", false);
        enemyAnimator.SetBool("isWalking", true);

        // ��������
        t = 0f;
        while (t < blackFadeDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t / blackFadeDuration));
            yield return null;
        }
        blackScreen.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    // �������Ѳ�ߵ�����
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
