using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    // 存档的 Key
    private const string PREF_KEY_MAX_LEVEL = "MaxLevelReached";

    void Awake()
    {
        // 单例模式，保证全局唯一
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景也不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 获取当前已解锁的最大关卡索引（从1开始）
    /// </summary>
    public int GetUnlockedLevel()
    {
        // 默认解锁第 1 关
        return PlayerPrefs.GetInt(PREF_KEY_MAX_LEVEL, 1);
    }

    /// <summary>
    /// 解锁下一关
    /// </summary>
    /// <param name="currentLevelCompleted">当前刚打通的关卡索引</param>
    public void CompleteLevel(int currentLevelCompleted)
    {
        int maxReached = GetUnlockedLevel();

        // 如果通关的是当前最新的关卡，那么就解锁下一关
        if (currentLevelCompleted >= maxReached)
        {
            int nextLevel = currentLevelCompleted + 1;
            PlayerPrefs.SetInt(PREF_KEY_MAX_LEVEL, nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"🎉 存档更新！已解锁第 {nextLevel} 关");
        }
    }

    // =========================================================
    // 🔥 测试作弊功能区 (Unity编辑器右键菜单)
    // =========================================================

    [ContextMenu("测试: 重置存档 (回到第1关)")]
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey(PREF_KEY_MAX_LEVEL);
        PlayerPrefs.Save();
        Debug.Log("🗑️ 存档已重置，进度清零。请重新运行游戏。");
    }

    [ContextMenu("测试: 一键解锁所有关卡 (设置到第99关)")]
    public void DebugUnlockAll()
    {
        // 直接设置到一个很大的数字，确保比你配置表里的关卡多，这样所有关卡都能点
        PlayerPrefs.SetInt(PREF_KEY_MAX_LEVEL, 99);
        PlayerPrefs.Save();
        Debug.Log("🔓 作弊成功！已解锁到第 99 关。请重新进入选关界面查看。");
    }
}