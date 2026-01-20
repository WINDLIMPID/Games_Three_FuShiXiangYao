using UnityEngine;
using System.Collections.Generic;

// 定义技能类型（对应策划案）
public enum SkillType
{
    Weapon,     // 主动武器 (天罡符, 落雷术)
    Passive,    // 被动属性 (灵足, 固元)
    Support     // 辅助 (护身金光)
}

[CreateAssetMenu(fileName = "SkillLibrary", menuName = "Game/Skill Library (All-in-One)")]
public class SkillLibrary : ScriptableObject
{
    [Header("全技能配置表")]
    public List<SkillDefinition> allSkills;

    // 辅助方法：根据ID获取技能数据
    public SkillDefinition GetSkillByID(int id)
    {
        return allSkills.Find(s => s.skillID == id);
    }
}

[System.Serializable]
public class SkillDefinition
{
    [Header("基础配置")]
    public int skillID;             // 唯一ID (建议：1001, 1002...)
    public string skillName;        // 技能名 (中文)
    public Sprite icon;             // UI图标
    public SkillType type;          // 类型

    [Tooltip("关联的武器预制体 (Weapon Prefab)")]
    public GameObject weaponPrefab; // 比如：天罡符的旋转中心，或者落雷管理器

    [Header("等级详情 (0级=Lv1, 1级=Lv2...)")]
    public List<SkillLevelInfo> levels;

    // 运行时数据（不存盘，仅游戏中使用）
    [System.NonSerialized]
    public int currentLevel = 0; // 当前等级 (0表示未学习)

    public bool IsMaxLevel => currentLevel >= levels.Count;

    // 获取下一级的描述
    public string GetNextLevelDesc()
    {
        if (IsMaxLevel) return "已满级";
        return levels[currentLevel].description;
    }
}

[System.Serializable]
public class SkillLevelInfo
{
    [TextArea(2, 3)]
    public string description;  // 升级文案，如 "数量+1"

    [Header("通用数值参数")]
    public float valueA;        // 参数A (如: 数量 / 伤害)
    public float valueB;        // 参数B (如: 转速 / 频率 / 范围)
    public float valueC;        // 参数C (备用)
}