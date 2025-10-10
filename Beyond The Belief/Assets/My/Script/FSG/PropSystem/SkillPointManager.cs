using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 技能点管理器：负责技能点的增加、消耗、UI更新及长按检测
public class SkillPointManager : MonoBehaviour
{
    // ====== 单例模式 ======
    public static SkillPointManager Instance;

    // ====== 技能点配置 ======
    public int currentSkillPoints = 0;
    public int maxSkillPoints = 3;
    public Image[] skillPointImages;

    // ====== 长按检测变量 ======
    private float holdTime = 2f;
    private float holdTimer = 0f;
    private bool isHolding = false;

    // ====== 检查点系统 ======
    private Dictionary<int, int> checkpointSkillPoints = new Dictionary<int, int>();
    private HashSet<int> recordedCheckpoints = new HashSet<int>(); // 记录已经保存过的检查点
    private int currentCheckpointIndex = -1;

    // ====== 事件系统 ======
    public static event System.Action<int> OnSkillPointsChanged;
    public static event System.Action<int> OnCheckpointSaved;

    // ====== 初始化单例 ======
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        // ❌ 移除自动保存检查点0
        // 检查点将在 CanStartAttack 开启后由 EnemyAttackController 保存
        Debug.Log($"🎯 技能点管理器初始化完成，当前技能点: {currentSkillPoints}，等待 CanStartAttack 开启后保存检查点");
    }

    // ====== 每帧检测长按逻辑 ======
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            isHolding = true;
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                UseSkillPoint();
                holdTimer = 0f;
                isHolding = false;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isHolding = false;
            holdTimer = 0f;
        }
    }

    // ====== 检查点相关方法 ======

    /// <summary>
    /// 保存当前技能点到检查点（只在第一次进入时保存）
    /// </summary>
    /// <param name="checkpointIndex">检查点索引</param>
    /// <param name="forceSave">是否强制保存</param>
    public void SaveCheckpointState(int checkpointIndex, bool forceSave = false)
    {
        // 如果不是强制保存，且已经记录过这个检查点，则跳过
        if (!forceSave && recordedCheckpoints.Contains(checkpointIndex))
        {
            Debug.Log($"⏭️ 检查点 {checkpointIndex} 已记录过，跳过保存");
            return;
        }

        if (checkpointSkillPoints.ContainsKey(checkpointIndex))
        {
            checkpointSkillPoints[checkpointIndex] = currentSkillPoints;
        }
        else
        {
            checkpointSkillPoints.Add(checkpointIndex, currentSkillPoints);
        }

        // 标记这个检查点已经记录过
        recordedCheckpoints.Add(checkpointIndex);
        currentCheckpointIndex = checkpointIndex;

        Debug.Log($"💾 检查点 {checkpointIndex} 技能点已保存: {currentSkillPoints} (首次记录)");
        OnCheckpointSaved?.Invoke(checkpointIndex);
    }

    /// <summary>
    /// 恢复到指定检查点的技能点状态
    /// </summary>
    public void RestoreToCheckpoint(int checkpointIndex)
    {
        if (checkpointSkillPoints.ContainsKey(checkpointIndex))
        {
            int savedSkillPoints = checkpointSkillPoints[checkpointIndex];
            SetSkillPoints(savedSkillPoints);

            Debug.Log($"🔄 技能点已恢复到检查点 {checkpointIndex} 的状态: {savedSkillPoints}");
        }
        else
        {
            Debug.LogWarning($"⚠️ 未找到检查点 {checkpointIndex} 的技能点记录，使用默认值");
            SetSkillPoints(0);
        }

        currentCheckpointIndex = checkpointIndex;
    }

    /// <summary>
    /// 检查是否已经记录过某个检查点
    /// </summary>
    public bool HasCheckpointRecorded(int checkpointIndex)
    {
        return recordedCheckpoints.Contains(checkpointIndex);
    }

    /// <summary>
    /// 强制重新记录当前检查点（慎用）
    /// </summary>
    public void ForceRerecordCheckpoint(int checkpointIndex)
    {
        if (checkpointSkillPoints.ContainsKey(checkpointIndex))
        {
            checkpointSkillPoints[checkpointIndex] = currentSkillPoints;
        }
        else
        {
            checkpointSkillPoints.Add(checkpointIndex, currentSkillPoints);
        }

        recordedCheckpoints.Add(checkpointIndex);
        Debug.Log($"🔁 强制重新记录检查点 {checkpointIndex}: {currentSkillPoints}");
    }

    /// <summary>
    /// 重置检查点记录状态（用于重新开始游戏等）
    /// </summary>
    public void ResetCheckpointRecords()
    {
        recordedCheckpoints.Clear();
        checkpointSkillPoints.Clear();
        currentCheckpointIndex = -1;
        Debug.Log("🔄 所有检查点记录已重置");
    }

    /// <summary>
    /// 手动初始化检查点0（在CanStartAttack开启时调用）
    /// </summary>
    public void InitializeCheckpointZero()
    {
        if (currentCheckpointIndex == -1) // 只在未初始化时执行
        {
            SaveCheckpointState(0, true);
            Debug.Log($"🎯 检查点0初始化完成: {currentSkillPoints}技能点");
        }
        else
        {
            Debug.Log($"⏭️ 检查点0已初始化，跳过");
        }
    }

    /// <summary>
    /// 获取当前检查点的技能点数量
    /// </summary>
    public int GetCurrentCheckpointSkillPoints()
    {
        if (checkpointSkillPoints.ContainsKey(currentCheckpointIndex))
            return checkpointSkillPoints[currentCheckpointIndex];
        return 0;
    }

    /// <summary>
    /// 获取指定检查点的技能点数量
    /// </summary>
    public int GetCheckpointSkillPoints(int checkpointIndex)
    {
        if (checkpointSkillPoints.ContainsKey(checkpointIndex))
            return checkpointSkillPoints[checkpointIndex];
        return 0;
    }

    // ====== 技能点基础操作 ======

    /// <summary>
    /// 增加技能点
    /// </summary>
    public void AddSkillPoint()
    {
        if (DialogueManager1.AnyDialogueActive) return;

        if (currentSkillPoints < maxSkillPoints)
        {
            currentSkillPoints++;
            UpdateSkillPointUI();
            Debug.Log($"➕ 技能点增加，当前: {currentSkillPoints}/{maxSkillPoints}");
        }
        else
        {
            Debug.Log("⛔ 技能点已达上限");
        }
    }

    /// <summary>
    /// 设置技能点数量
    /// </summary>
    public void SetSkillPoints(int value)
    {
        if (DialogueManager1.AnyDialogueActive) return;

        int previousValue = currentSkillPoints;
        currentSkillPoints = Mathf.Clamp(value, 0, maxSkillPoints);
        UpdateSkillPointUI();

        //Debug.Log($"🎯 技能点设置: {previousValue} → {currentSkillPoints}");
    }

    /// <summary>
    /// 消耗技能点
    /// </summary>
    public void UseSkillPoint()
    {
        if (DialogueManager1.AnyDialogueActive) return;

        if (currentSkillPoints > 0)
        {
            currentSkillPoints--;
            UpdateSkillPointUI();
            Debug.Log($"➖ 使用了一个技能点，剩余: {currentSkillPoints}");
        }
        else
        {
            Debug.Log("⛔ 没有可用的技能点");
        }
    }

    /// <summary>
    /// 检查是否可以消耗技能点
    /// </summary>
    public bool CanUseSkillPoint()
    {
        return currentSkillPoints > 0 && !DialogueManager1.AnyDialogueActive;
    }

    /// <summary>
    /// 获取当前技能点数量
    /// </summary>
    public int GetCurrentSkillPoints()
    {
        return currentSkillPoints;
    }

    /// <summary>
    /// 获取技能点上限
    /// </summary>
    public int GetMaxSkillPoints()
    {
        return maxSkillPoints;
    }

    // ====== UI更新 ======
    private void UpdateSkillPointUI()
    {
        if (skillPointImages == null || skillPointImages.Length == 0)
        {
            Debug.LogWarning("⚠️ 技能点UI图像数组未设置");
            return;
        }

        for (int i = 0; i < skillPointImages.Length; i++)
        {
            if (skillPointImages[i] != null)
            {
                skillPointImages[i].enabled = i < currentSkillPoints;
            }
        }

        OnSkillPointsChanged?.Invoke(currentSkillPoints);
    }

    // ====== 调试方法 ======

    [ContextMenu("打印所有检查点状态")]
    public void PrintAllCheckpoints()
    {
        Debug.Log("=== 所有检查点技能点状态 ===");
        if (checkpointSkillPoints.Count == 0)
        {
            Debug.Log("没有保存的检查点");
            return;
        }

        foreach (var checkpoint in checkpointSkillPoints)
        {
            string recordedStatus = recordedCheckpoints.Contains(checkpoint.Key) ? "已记录" : "未记录";
            Debug.Log($"检查点 {checkpoint.Key}: {checkpoint.Value} 技能点 [{recordedStatus}]");
        }
    }

    [ContextMenu("清除检查点记录状态")]
    public void ClearRecordedStatus()
    {
        recordedCheckpoints.Clear();
        Debug.Log("✅ 检查点记录状态已清除，下次进入时会重新记录");
    }

    [ContextMenu("初始化检查点0")]
    public void ManualInitializeCheckpointZero()
    {
        InitializeCheckpointZero();
    }
}