using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFollowPlayer : MonoBehaviour
{
    public Transform player;       // 主角
    public Transform center;       // 空心区域中心
    public float radius = 5f;      // 空心区域半径
    public float moveSpeed = 5f;   // 移动速度

    [Header("可变楼层设置")]
    public List<float> floorHeightThresholds; // 楼层分界Y坐标（n-1个阈值）
    public List<float> bossHeightOffsets;     // 每层对应Boss高度偏移（长度 = 楼层数量）

    [Header("锁定设置")]
    [Tooltip("指定Boss在哪一层锁定（从0开始计数，例如0=第一层，1=第二层）")]
    public int lockAtFloorIndex = 0; // 指定在哪层锁定
    public float heightLockTolerance = 0.1f; // 到达指定高度的容差范围

    [Header("Boss状态检测")]
    public BossController bossController; // 用于检测晕厥状态

    [Header("晕厥位移设置")]
    [Tooltip("晕厥时Boss的目标高度")]
    public float stunTargetHeight = 15f;
    [Tooltip("移动到晕厥高度的持续时间")]
    public float stunLiftDuration = 1.0f;
    [Tooltip("从晕厥高度返回的持续时间")]
    public float stunReturnDuration = 0.8f;

    private bool heightLocked = false;
    private float lockedHeight;
    private bool isInStunMode = false;
    private Vector3 preStunPosition;
    private Coroutine stunMovementCoroutine;

    void Start()
    {
        // 自动获取BossController
        if (bossController == null)
            bossController = GetComponent<BossController>();

        //Debug.Log($"🔍 BossFollowPlayer.Start:");
        //Debug.Log($"  当前物体: {gameObject.name}");
        //Debug.Log($"  BossController: {(bossController != null ? "已找到" : "未找到")}");

        if (bossController == null)
        {
            // 尝试在父物体或子物体中查找
            bossController = GetComponentInParent<BossController>();
            if (bossController != null)
                Debug.Log($"  在父物体中找到BossController: {bossController.gameObject.name}");
        }
    }

    void Update()
    {
        // ✅ 如果处于晕厥模式，不执行正常移动
        if (isInStunMode)
        {
            return;
        }

        // ✅ 检查Boss状态：如果晕厥或死亡，停止移动
        if (bossController != null && (bossController.IsStunned || bossController.IsDead))
        {
            return;
        }

        // 正常移动逻辑
        ExecuteNormalMovement();
    }

    private void ExecuteNormalMovement()
    {
        // 1️⃣ 水平移动
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dirFromCenter = targetPos - center.position;
        if (dirFromCenter.magnitude > radius)
            dirFromCenter = dirFromCenter.normalized * radius;
        Vector3 bossTarget = center.position + dirFromCenter;

        // 2️⃣ 如果还没锁定，根据玩家楼层确定目标高度
        float targetY = heightLocked ? lockedHeight : GetBossHeight(player.position.y);
        bossTarget.y = targetY;

        // 3️⃣ 移动Boss
        transform.position = Vector3.MoveTowards(transform.position, bossTarget, moveSpeed * Time.deltaTime);

        // 4️⃣ 检查是否到达指定楼层（锁定）
        if (!heightLocked && Mathf.Abs(transform.position.y - GetFloorHeight(lockAtFloorIndex)) <= heightLockTolerance)
        {
            lockedHeight = GetFloorHeight(lockAtFloorIndex);
            heightLocked = true;
        }

        // 5️⃣ 始终朝向玩家
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);
    }

    // ✅ 新增：Boss开始晕厥时调用
    public void OnBossStunStart()
    {
        if (isInStunMode) return;

        isInStunMode = true;
        preStunPosition = transform.position;

        //Debug.Log($"✅ OnBossStunStart 被调用");
        //Debug.Log($"   当前物体: {gameObject.name}");
        //Debug.Log($"   当前位置: {transform.position}");
        //Debug.Log($"   目标高度: {stunTargetHeight}");

        if (stunMovementCoroutine != null)
            StopCoroutine(stunMovementCoroutine);

        stunMovementCoroutine = StartCoroutine(MoveToStunHeight());
    }

    // ✅ 新增：Boss结束晕厥时调用
    public void OnBossStunEnd()
    {
        if (!isInStunMode) return;

        // 停止之前的移动协程
        if (stunMovementCoroutine != null)
            StopCoroutine(stunMovementCoroutine);

        // 开始返回正常位置
        stunMovementCoroutine = StartCoroutine(ReturnFromStunHeight());

        //Debug.Log("结束晕厥位移，开始返回");
    }

    private IEnumerator MoveToStunHeight()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, stunTargetHeight, startPos.z);
        float elapsed = 0f;

        //Debug.Log($"🚀 开始移动协程: {startPos.y} -> {targetPos.y}");

        while (elapsed < stunLiftDuration)
        {
            float t = elapsed / stunLiftDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;

            // 实时输出位置变化
            if (Mathf.FloorToInt(elapsed * 10) % 2 == 0) // 每0.2秒输出一次
            {
                //Debug.Log($"  移动中: {transform.position.y} (进度: {t * 100:F1}%)");
            }
            yield return null;
        }

        transform.position = targetPos;
        //Debug.Log($"✅ 到达目标高度: {transform.position.y}");
    }

    private IEnumerator ReturnFromStunHeight()
    {
        Vector3 startPos = transform.position;

        // 计算返回的目标位置（基于玩家当前楼层）
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

        //Debug.Log("已返回正常高度，晕厥位移结束");
    }

    float GetBossHeight(float playerY)
    {
        // 默认取最后一层偏移
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

    float GetFloorHeight(int floorIndex)
    {
        floorIndex = Mathf.Clamp(floorIndex, 0, bossHeightOffsets.Count - 1);
        return center.position.y + bossHeightOffsets[floorIndex];
    }

    // 公开方法用于外部控制
    public void SetHeightLocked(bool locked)
    {
        heightLocked = locked;
    }

    // 公开属性
    public bool IsHeightLocked => heightLocked;
    public float CurrentLockedHeight => lockedHeight;
    public bool IsInStunMode => isInStunMode;
}