using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI 组件")]
    public Slider expSlider;
    public TextMeshProUGUI levelText;

    [Header("数值配置")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 🔥🔥🔥 新增：战斗场景开始，切换战斗 BGM 🔥🔥🔥
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("BattleBGM");
        }
        UpdateUI();
    }

    // 🔥 修改重点：这里加入了经验倍率计算
    public void AddExp(int amount)
    {
        // 1. 检查玩家状态
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null && hp.isDead) return;
        }

        // 2. 🔥 获取当前关卡的经验倍率
        float multiplier = 1.0f;
        if (GlobalConfig.Instance != null && GlobalConfig.Instance.currentLevelConfig != null)
        {
            multiplier = GlobalConfig.Instance.currentLevelConfig.expGainMultiplier;
        }

        // 3. 🔥 计算最终经验 (基础值 * 倍率)
        // 例如：第8关倍率是8.0，原本一只怪给10经验，现在给80经验！
        int finalAmount = Mathf.RoundToInt(amount * multiplier);

        // 4. 加经验并检查升级
        currentExp += finalAmount;
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;

        // 难度曲线：下一级所需经验 * 1.2
        // 这样虽然每级需要的经验变多了，但因为第8关掉落的经验也翻倍了，所以升级速度依然很快
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);

        // 🔥 2. 升级特效 (在玩家脚下)
        if (VFXManager.Instance != null)
        {
            // 假设 PlayerController 是单例，或者你用 GameObject.FindWithTag("Player")
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                VFXManager.Instance.PlayVFX("LevelUp", player.transform.position, Quaternion.Euler(-90, 0, 0));
            }
        }
        // 对接升级管理器
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.TriggerLevelUp();
        }
        else
        {
            Debug.LogError("❌ 场景里找不到 UpgradeManager！请检查是否挂载了脚本。");
        }

        // 修改 Log 方便调试，能看到当前是第几关的倍率
        Debug.Log($"🎉 境界提升！当前境界: {currentLevel} (关卡经验倍率生效中)");
    }

    void UpdateUI()
    {
        if (expSlider != null)
        {
            // 防止分母为0报错
            if (expToNextLevel > 0)
                expSlider.value = (float)currentExp / expToNextLevel;
            else
                expSlider.value = 1;
        }

        if (levelText != null)
        {
            levelText.text = "境界 " + currentLevel;
        }
    }
}