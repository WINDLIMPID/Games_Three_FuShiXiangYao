using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI 引用")]
    public GameObject levelUpPanel;
    public UI_SkillCard[] skillCards;

    [Header("配置引用")]
    public SkillLibrary skillLibrary;

    private void Awake()
    {
        Instance = this;
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        ResetAllSkillsData();
    }

    private void ResetAllSkillsData()
    {
        if (skillLibrary == null || skillLibrary.allSkills == null) return;
        foreach (var skill in skillLibrary.allSkills)
        {
            skill.currentLevel = 0;
        }
    }

    public void TriggerLevelUp()
    {
        // 1. 检查是否全满级
        bool hasAvailableSkills = skillLibrary.allSkills.Any(s => !s.IsMaxLevel);

        if (!hasAvailableSkills)
        {
            Debug.Log("🎉 所有技能已满级！触发 50% 回血奖励！");
            HealPlayerOnMaxLevel();
            return;
        }

        // 2. 正常升级弹窗
        if (levelUpPanel != null)
        {
            Time.timeScale = 0;
            levelUpPanel.SetActive(true);
            RefreshSkillOptions();
        }
    }

    // 🔥🔥🔥 修改：按 50% 最大血量回血 🔥🔥🔥
    private void HealPlayerOnMaxLevel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null)
            {
                // 计算 50% 的血量
                int healAmount = Mathf.RoundToInt(hp.maxHealth * 0.5f);

                // 执行治疗
                hp.Heal(healAmount);

                Debug.Log($"✨ 极限突破！最大血量: {hp.maxHealth}, 回复 50%: {healAmount}");

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("LevelUp");
            }
        }
    }

    private void RefreshSkillOptions()
    {
        List<SkillDefinition> available = skillLibrary.allSkills
            .Where(s => !s.IsMaxLevel).ToList();

        // 洗牌
        for (int i = 0; i < available.Count; i++)
        {
            SkillDefinition temp = available[i];
            int randomIndex = Random.Range(i, available.Count);
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }

        // 填充 UI
        for (int i = 0; i < skillCards.Length; i++)
        {
            if (i < available.Count)
            {
                SkillDefinition skill = available[i];
                skillCards[i].gameObject.SetActive(true);
                skillCards[i].Setup(
                    skill.icon,
                    skill.skillName,
                    "境界" + (skill.currentLevel + 1),
                    skill.GetNextLevelDesc(),
                    () => {
                        skill.currentLevel++;
                        ApplySkillEffect(skill);
                    }
                );
            }
            else skillCards[i].gameObject.SetActive(false);
        }
    }

    public void ApplySkillEffect(SkillDefinition skill)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        TalismanOrbit orbit = player.GetComponent<TalismanOrbit>();
        PlayerController pc = player.GetComponent<PlayerController>();
        Health hp = player.GetComponent<Health>();

        float valA = skill.levels[skill.currentLevel - 1].valueA;

        switch (skill.skillID)
        {
            case 101: orbit.count += (int)valA; break;
            case 102: orbit.rotateSpeed += valA; break;
            case 103: orbit.radius += valA; orbit.RebuildFormation(); break;

            // ✅ 确保这里是 orbit.AddDamage，而不是 Debug.Log
            case 104:
                if (orbit != null) orbit.AddDamage((int)valA);
                break;

            case 105: if (pc) pc.moveSpeed += valA; break;
            case 106: if (hp) { hp.maxHealth += (int)valA; hp.currentHealth += (int)valA; hp.UpdateUI(); } break;
            case 107: if (pc) pc.expMultiplier += valA; break;
        }

        levelUpPanel.SetActive(false);
        Time.timeScale = 1;
    }
}