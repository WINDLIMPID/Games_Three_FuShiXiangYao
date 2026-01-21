using UnityEngine;
using System.Collections.Generic;

public class UI_HUD_SkillManager : MonoBehaviour
{
    public static UI_HUD_SkillManager Instance;

    [Header("把那7个 Image 拖进来")]
    public UI_HUD_SkillSlot[] skillSlots;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 游戏开始时，清空所有显示
        foreach (var slot in skillSlots)
        {
            if (slot != null) slot.InitSlot();
        }
    }

    // 🔥 核心方法：供外部调用刷新界面
    public void UpdateSkillDisplay(SkillDefinition skill)
    {
        if (skill == null) return;

        // 1. 先找找这个技能是不是已经在 UI 上了
        foreach (var slot in skillSlots)
        {
            if (slot.currentSkillID == skill.skillID)
            {
                // 找到了！更新等级
                slot.UpdateSlot(skill.currentLevel);
                return;
            }
        }

  
        Debug.LogWarning("UI技能槽位已满，无法显示新技能图标！");
    }
}