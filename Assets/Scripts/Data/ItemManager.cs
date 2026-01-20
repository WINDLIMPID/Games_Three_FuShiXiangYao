using UnityEngine;
using System;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    // --- 存档 Key ---
    private const string PREF_LINGZHI = "Item_LingZhi";
    private const string PREF_THUNDER = "Item_ThunderCharm"; // 🔥 新增：雷符存档Key

    // --- 内存变量 ---
    private int _lingZhiCount = 0;
    private int _thunderCount = 0; // 🔥 新增：雷符数量

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景不销毁
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化时读取存档
        LoadItems();
    }

    // ==========================================
    // 灵芝相关 (保持不变)
    // ==========================================
    public int GetLingZhiCount() { return _lingZhiCount; }

    public void AddLingZhi(int amount)
    {
        _lingZhiCount += amount;
        SaveItems();
        Debug.Log($"📦 获得灵芝！当前数量: {_lingZhiCount}");
    }

    public bool UseLingZhi(int amount = 1)
    {
        if (_lingZhiCount >= amount)
        {
            _lingZhiCount -= amount;
            SaveItems();
            return true;
        }
        return false;
    }

    // ==========================================
    // 🔥🔥🔥 新增：雷神符 核心逻辑 🔥🔥🔥
    // ==========================================

    public int GetThunderCount()
    {
        return _thunderCount;
    }

    public void AddThunder(int amount)
    {
        _thunderCount += amount;
        SaveItems();
        Debug.Log($"⚡ 获得雷神符！当前数量: {_thunderCount}");
    }

    public bool UseThunder(int amount = 1)
    {
        if (_thunderCount >= amount)
        {
            _thunderCount -= amount;
            SaveItems();
            return true;
        }
        else
        {
            Debug.Log("❌ 雷神符不足！");
            return false;
        }
    }

    // --- 内部存档逻辑 (已更新) ---

    private void SaveItems()
    {
        PlayerPrefs.SetInt(PREF_LINGZHI, _lingZhiCount);
        PlayerPrefs.SetInt(PREF_THUNDER, _thunderCount); // 🔥 保存雷符
        PlayerPrefs.Save();
    }

    private void LoadItems()
    {
        _lingZhiCount = PlayerPrefs.GetInt(PREF_LINGZHI, 0);
        _thunderCount = PlayerPrefs.GetInt(PREF_THUNDER, 0); // 🔥 读取雷符
    }
}