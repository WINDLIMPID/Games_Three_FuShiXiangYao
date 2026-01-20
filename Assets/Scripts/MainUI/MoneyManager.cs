using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    // 事件：通知 UI 刷新
    public event Action<int> OnCoinChanged;

    private int _currentCoins = 0;

    // 配置
    private const int MAX_COINS = 99999;
    private const int MIN_COINS = 0;
    private const string PREF_COINS = "Player_Coins";

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
            return;
        }

        // 立刻读档
        LoadCoins();
    }

    void Start()
    {
        // 再次广播，确保 UI 同步
        OnCoinChanged?.Invoke(_currentCoins);
    }

    // --- 公开方法 ---

    public int GetCoins()
    {
        return _currentCoins;
    }

    public void AddCoins(int amount)
    {
        UpdateCoinValue(_currentCoins + amount);
    }

    public bool SpendCoins(int amount)
    {
        if (_currentCoins >= amount)
        {
            UpdateCoinValue(_currentCoins - amount);
            return true;
        }
        else
        {
            Debug.LogWarning("❌ 余额不足");
            return false;
        }
    }

    // 🔥🔥🔥 新增：重置金币 (给按钮绑定的方法) 🔥🔥🔥
    public void ResetCoins()
    {
        UpdateCoinValue(0); // 直接设为 0
        Debug.Log("🗑️ 金币已重置为 0");
    }

    // --- 内部逻辑 ---

    private void UpdateCoinValue(int newValue)
    {
        _currentCoins = Mathf.Clamp(newValue, MIN_COINS, MAX_COINS);

        PlayerPrefs.SetInt(PREF_COINS, _currentCoins);
        PlayerPrefs.Save();

        OnCoinChanged?.Invoke(_currentCoins); // 通知 UI
    }

    private void LoadCoins()
    {
        _currentCoins = PlayerPrefs.GetInt(PREF_COINS, 0);
    }
}