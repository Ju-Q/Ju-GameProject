using UnityEngine;
using UnityEngine.UI;

// 技能点管理器：负责技能点的增加、消耗、UI更新及长按检测
public class SkillPointManager : MonoBehaviour
{
    // ====== 单例模式 ======
    public static SkillPointManager Instance; // 全局唯一实例，方便其他脚本访问

    // ====== 技能点配置 ======
    public int currentSkillPoints = 0;  // 当前拥有的技能点数
    public int maxSkillPoints = 3;     // 技能点上限
    public Image[] skillPointImages;   // UI显示的技能点图标（如3个Image组成的数组）

    // ====== 长按检测变量 ======
    private float holdTime = 2f;       // 触发长按的所需时间（秒）
    private float holdTimer = 0f;      // 计时器，记录鼠标左键按住的时间
    private bool isHolding = false;    // 是否正在长按

    // ====== 初始化单例 ======
    private void Awake()
    {
        if (Instance == null)
            Instance = this; // 确保场景中只有一个实例
    }

    // ====== 每帧检测长按逻辑 ======
    private void Update()
    {
        if (Input.GetMouseButton(0)) // 鼠标左键按住时
        {
            isHolding = true;
            holdTimer += Time.deltaTime; // 累加按住时间

            // 如果按住时间超过阈值，触发技能点消耗
            if (holdTimer >= holdTime)
            {
                UseSkillPoint();    // 消耗技能点
                holdTimer = 0f;       // 重置计时器
                isHolding = false;    // 结束长按状态
            }
        }
        else if (Input.GetMouseButtonUp(0)) // 鼠标左键松开时
        {
            isHolding = false;
            holdTimer = 0f; // 重置计时器
        }
    }

    // ====== 增加技能点 ======
    public void AddSkillPoint()
    {
        if (currentSkillPoints < maxSkillPoints)
        {
            currentSkillPoints++;
            UpdateSkillPointUI(); // 更新UI显示
        }
    }

    // ====== 强制设置技能点数值 ======
    public void SetSkillPoints(int value)
    {
        // 限制数值在0和maxSkillPoints之间
        currentSkillPoints = Mathf.Clamp(value, 0, maxSkillPoints);
        UpdateSkillPointUI();
    }

    // ====== 消耗技能点 ======
    private void UseSkillPoint()
    {
        if (currentSkillPoints > 0)
        {
            currentSkillPoints--;
            UpdateSkillPointUI();
            Debug.Log("使用了一个技能点，剩余：" + currentSkillPoints);
            // 这里可以添加技能释放的具体逻辑（如播放特效、触发技能等）
        }
    }

    // ====== 更新UI显示 ======
    private void UpdateSkillPointUI()
    {
        // 遍历所有技能点图标，根据当前技能点数显示/隐藏
        for (int i = 0; i < skillPointImages.Length; i++)
        {
            skillPointImages[i].enabled = i < currentSkillPoints;
            // 例如：currentSkillPoints=2时，前两个图标显示，第三个隐藏
        }
    }
}