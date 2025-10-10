using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFollowPlayer : MonoBehaviour
{
    [Header("目标与中心")]
    public Transform player;       // 主角
    public Transform center;       // 空心区域中心
    public float moveSpeed = 5f;   // 移动速度

    [Header("多层设置")]
    [Tooltip("楼层分界Y坐标 (n-1个阈值)")]
    public List<float> floorHeightThresholds;
    [Tooltip("每层Boss高度偏移 (长度 = 楼层数量)")]
    public List<float> bossHeightOffsets;
    [Tooltip("每层Boss移动半径 (长度 =楼层数量)")]
    public List<float> floorRadiusList;

    [Header("锁定设置")]
    [Tooltip("指定Boss在哪一层锁定 (0 = 第一层)")]
    public int lockAtFloorIndex = 0;
    public float heightLockTolerance = 0.1f;

    [Header("Boss状态检测")]
    public BossController bossController;

    [Header("晕厥位移设置")]
    public float stunTargetHeight = 15f;
    public float stunLiftDuration = 1.0f;
    public float stunReturnDuration = 0.8f;

    [Header("激活控制")]
    [Tooltip("是否允许Boss开始跟随玩家")]
    public bool isActive = false; // 默认 false，未触发

    // 内部状态
    private bool heightLocked = false;
    private float lockedHeight;
    private bool isInStunMode = false;
    private Vector3 preStunPosition;
    private Coroutine stunMovementCoroutine;
    private float lockedRadius; // 锁定楼层的半径

    // 属性
    public bool IsHeightLocked => heightLocked;
    public float CurrentLockedHeight => lockedHeight;
    public bool IsInStunMode => isInStunMode;

    private void Start()
    {
        if (bossController == null)
            bossController = GetComponent<BossController>() ?? GetComponentInParent<BossController>();
    }

    private void Update()
    {
        if (!isActive) return; // ✅ 未激活，跳过
        if (isInStunMode) return;
        if (bossController != null && (bossController.IsStunned || bossController.IsDead)) return;

        ExecuteNormalMovement();
    }

    private void ExecuteNormalMovement()
    {
        // 1️⃣ 获取当前半径
        float currentRadius = heightLocked ? lockedRadius : GetRadiusByPlayerHeight(player.position.y);

        // 2️⃣ 水平移动
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dirFromCenter = targetPos - center.position;
        if (dirFromCenter.magnitude > currentRadius)
            dirFromCenter = dirFromCenter.normalized * currentRadius;
        Vector3 bossTarget = center.position + dirFromCenter;

        // 3️⃣ 高度
        float targetY = heightLocked ? lockedHeight : GetBossHeight(player.position.y);
        bossTarget.y = targetY;

        // 4️⃣ 移动Boss
        transform.position = Vector3.MoveTowards(transform.position, bossTarget, moveSpeed * Time.deltaTime);

        // 5️⃣ 检查是否到达指定锁定楼层
        if (!heightLocked && Mathf.Abs(transform.position.y - GetFloorHeight(lockAtFloorIndex)) <= heightLockTolerance)
        {
            lockedHeight = GetFloorHeight(lockAtFloorIndex);
            lockedRadius = GetRadiusByFloorIndex(lockAtFloorIndex); // 锁定半径
            heightLocked = true;
        }

        // 6️⃣ 朝向玩家
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);
    }

    // ========== 获取楼层半径和高度 ==========

    private float GetRadiusByFloorIndex(int floorIndex)
    {
        floorIndex = Mathf.Clamp(floorIndex, 0, floorRadiusList.Count - 1);
        return floorRadiusList[floorIndex];
    }

    private float GetBossHeight(float playerY)
    {
        float height = bossHeightOffsets[bossHeightOffsets.Count - 1];
        for (int i = 0; i < floorHeightThresholds.Count; i++)
        {
            if (playerY < floorHeightThresholds[i])
            {
                height = bossHeightOffsets[i];
                break;
            }
        }
        return center.position.y + height;
    }

    private float GetFloorHeight(int floorIndex)
    {
        floorIndex = Mathf.Clamp(floorIndex, 0, bossHeightOffsets.Count - 1);
        return center.position.y + bossHeightOffsets[floorIndex];
    }

    private float GetRadiusByPlayerHeight(float playerY)
    {
        int floorIndex = 0;
        for (int i = 0; i < floorHeightThresholds.Count; i++)
        {
            if (playerY < floorHeightThresholds[i])
            {
                floorIndex = i;
                break;
            }
            else
            {
                floorIndex = bossHeightOffsets.Count - 1;
            }
        }
        floorIndex = Mathf.Clamp(floorIndex, 0, floorRadiusList.Count - 1);
        return floorRadiusList[floorIndex];
    }

    // ========== 晕厥逻辑 ==========

    public void OnBossStunStart()
    {
        if (isInStunMode) return;
        isInStunMode = true;
        preStunPosition = transform.position;

        if (stunMovementCoroutine != null)
            StopCoroutine(stunMovementCoroutine);

        stunMovementCoroutine = StartCoroutine(MoveToStunHeight());
    }

    public void OnBossStunEnd()
    {
        if (!isInStunMode) return;

        if (stunMovementCoroutine != null)
            StopCoroutine(stunMovementCoroutine);

        stunMovementCoroutine = StartCoroutine(ReturnFromStunHeight());
    }

    private IEnumerator MoveToStunHeight()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, stunTargetHeight, startPos.z);
        float elapsed = 0f;

        while (elapsed < stunLiftDuration)
        {
            float t = elapsed / stunLiftDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    private IEnumerator ReturnFromStunHeight()
    {
        Vector3 startPos = transform.position;
        float targetY = heightLocked ? lockedHeight : GetBossHeight(player.position.y);
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;
        while (elapsed < stunReturnDuration)
        {
            float t = elapsed / stunReturnDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isInStunMode = false;
    }

    // ========== 公共接口 ==========

    public void SetHeightLocked(bool locked)
    {
        heightLocked = locked;
    }

    public void ActivateBossFollow()
    {
        isActive = true;
    }
}
