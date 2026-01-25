using UnityEngine;
using System.IO;
using System;

[Serializable]
public class PlayerData
{
    public int coin = 0;              // 金币
    public int unlockedLevel = 1;     // 已解锁最大关卡
    public int highScore = 0;         // 最高分
    public bool isTutorialComplete = false; // 新手引导

    // 🔥🔥🔥 新增：道具数据存入账号存档 🔥🔥🔥
    public int lingZhi = 0;  // 灵芝数量
    public int thunder = 0;  // 雷符数量
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public PlayerData playerData = new PlayerData();
    private string currentUsername = "";

    // 金币变化事件
    public event Action<int> OnCoinChanged;
    // 🔥 道具变化事件 (可选，方便UI刷新)
    public event Action OnItemChanged;

    void Awake()
    {
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

    // ==========================================
    // 💰 金币逻辑
    // ==========================================
    public int GetCoin() => playerData.coin;

    public void AddMoney(int amount)
    {
        playerData.coin += amount;
        Save();
        OnCoinChanged?.Invoke(playerData.coin);
    }

    public bool SpendMoney(int amount)
    {
        if (playerData.coin >= amount)
        {
            playerData.coin -= amount;
            Save();
            OnCoinChanged?.Invoke(playerData.coin);
            return true;
        }
        return false;
    }

    // ==========================================
    // 📦 道具逻辑 (接管 ItemManager 的数据)
    // ==========================================

    // --- 灵芝 ---
    public int GetLingZhi() => playerData.lingZhi;

    public void AddLingZhi(int amount)
    {
        playerData.lingZhi += amount;
        Save();
        OnItemChanged?.Invoke();
    }

    public bool UseLingZhi(int amount)
    {
        if (playerData.lingZhi >= amount)
        {
            playerData.lingZhi -= amount;
            Save();
            OnItemChanged?.Invoke();
            return true;
        }
        return false;
    }

    // --- 雷符 ---
    public int GetThunder() => playerData.thunder;

    public void AddThunder(int amount)
    {
        playerData.thunder += amount;
        Save();
        OnItemChanged?.Invoke();
    }

    public bool UseThunder(int amount)
    {
        if (playerData.thunder >= amount)
        {
            playerData.thunder -= amount;
            Save();
            OnItemChanged?.Invoke();
            return true;
        }
        return false;
    }

    // ==========================================
    // 💾 存档/读档逻辑
    // ==========================================
    public void LoadUserData(string username)
    {
        currentUsername = username;
        string path = GetSavePath(username);

        if (File.Exists(path))
        {
            try
            {
                playerData = JsonUtility.FromJson<PlayerData>(File.ReadAllText(path));
            }
            catch
            {
                ResetToNewUser();
            }
        }
        else
        {
            ResetToNewUser();
            Save();
        }

        // 读档后通知 UI 刷新
        OnCoinChanged?.Invoke(playerData.coin);
        OnItemChanged?.Invoke();
    }

    private void ResetToNewUser()
    {
        playerData = new PlayerData();
        playerData.unlockedLevel = 1;
        playerData.coin = 0;
        playerData.lingZhi = 0; // 新号默认 0
        playerData.thunder = 0; // 新号默认 0
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(currentUsername)) return;
        File.WriteAllText(GetSavePath(currentUsername), JsonUtility.ToJson(playerData));
    }

    private string GetSavePath(string username) => Path.Combine(Application.persistentDataPath, $"save_{username}.json");

    // ==========================================
    // 🎮 游戏进度逻辑
    // ==========================================
    public int GetUnlockedLevel() => playerData.unlockedLevel;
    public int GetHighScore() => playerData.highScore;

    public void CompleteLevel(int currentLevelIndex)
    {
        if (currentLevelIndex >= playerData.unlockedLevel)
        {
            playerData.unlockedLevel = currentLevelIndex + 1;
            Save();
        }
    }

    public bool TrySaveHighScore(int score)
    {
        if (score > playerData.highScore)
        {
            playerData.highScore = score;
            Save();
            return true;
        }
        return false;
    }

    public bool IsTutorialComplete() => playerData.isTutorialComplete;

    public void CompleteTutorial()
    {
        playerData.isTutorialComplete = true;
        Save();
    }

    public void ApplyTestAccountConfig(AccountTier tier)
    {
        if (tier == AccountTier.None) return;
        if (tier == AccountTier.Senior)
        {
            playerData.unlockedLevel = 100;
            playerData.coin = 99999;
            playerData.isTutorialComplete = true;
            playerData.lingZhi = 99; // 测试号送道具
            playerData.thunder = 99;
        }
        Save();
        OnCoinChanged?.Invoke(playerData.coin);
        OnItemChanged?.Invoke();
    }
}