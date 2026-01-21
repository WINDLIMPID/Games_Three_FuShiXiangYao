using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // --- 存档 Key 定义 ---
    private const string PREF_KEY_MAX_LEVEL = "MaxLevelReached";
    private const string PREF_KEY_TUTORIAL = "IsTutorialFinished";
    private const string PREF_KEY_HIGH_SCORE = "EndlessHighScore";

    // 🔥 必须和 MainMenu.cs 里用的 Key 保持一致
    private const string PREF_KEY_STORY = "HasWatchedIntroStory";

    void Awake()
    {
        // 保证全局唯一且不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // 📖 原有功能：关卡解锁
    // =========================================================

    /// <summary>
    /// 获取当前已解锁的最大关卡索引（从1开始）
    /// </summary>
    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(PREF_KEY_MAX_LEVEL, 1);
    }

    /// <summary>
    /// 解锁下一关
    /// </summary>
    public void CompleteLevel(int currentLevelCompleted)
    {
        int maxReached = GetUnlockedLevel();
        // 如果通关的是当前最新的关卡，解锁下一关
        if (currentLevelCompleted >= maxReached)
        {
            int nextLevel = currentLevelCompleted + 1;
            PlayerPrefs.SetInt(PREF_KEY_MAX_LEVEL, nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"🎉 存档更新！已解锁第 {nextLevel} 关");
        }
    }

    // =========================================================
    // 📖 原有功能：排行榜分数
    // =========================================================

    /// <summary>
    /// 获取历史最高分
    /// </summary>
    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(PREF_KEY_HIGH_SCORE, 0);
    }

    /// <summary>
    /// 尝试保存最高分（只有比旧分数高才存）
    /// </summary>
    /// <returns>如果是新纪录返回 true</returns>
    public bool TrySaveHighScore(int newScore)
    {
        int currentHigh = GetHighScore();
        if (newScore > currentHigh)
        {
            PlayerPrefs.SetInt(PREF_KEY_HIGH_SCORE, newScore);
            PlayerPrefs.Save();
            Debug.Log($"🏆 新纪录诞生！旧分: {currentHigh} -> 新分: {newScore}");
            return true;
        }
        return false;
    }

    // =========================================================
    // 🛠️ 测试工具区 (右键点击组件使用)
    // =========================================================

    [ContextMenu("测试: 重置新手引导")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_TUTORIAL);
        PlayerPrefs.Save();
        Debug.Log("👶 新手引导已重置！");
    }

    [ContextMenu("测试: 重置最高分")]
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_HIGH_SCORE);
        PlayerPrefs.Save();
        Debug.Log("🏆 最高分已清零！");
    }

    // 🔥🔥🔥 新增：右键点击 SaveManager 组件就能看到这个选项 🔥🔥🔥
    [ContextMenu("测试: 重置剧情漫画 (变回新号)")]
    public void ResetStoryStatus()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_STORY);
        PlayerPrefs.Save();
        Debug.Log("📖 剧情漫画状态已重置！下次运行将自动播放漫画。");
    }

    [ContextMenu("测试: 彻底重置所有数据 (删库)")]
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("🗑️ 所有数据已清空，游戏回到初始状态。");
    }
}