using UnityEngine;
using System;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

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
    // 灵芝相关 (转接 SaveManager)
    // ==========================================
    public int GetLingZhiCount()
    {
        if (SaveManager.Instance != null)
            return SaveManager.Instance.GetLingZhi();
        return 0;
    }

    public void AddLingZhi(int amount)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddLingZhi(amount);
            Debug.Log($"📦 获得灵芝！当前数量: {SaveManager.Instance.GetLingZhi()}");
        }
    }

    public bool UseLingZhi(int amount = 1)
    {
        if (SaveManager.Instance != null)
        {
            return SaveManager.Instance.UseLingZhi(amount);
        }
        return false;
    }

    // ==========================================
    // 雷神符相关 (转接 SaveManager)
    // ==========================================

    public int GetThunderCount()
    {
        if (SaveManager.Instance != null)
            return SaveManager.Instance.GetThunder();
        return 0;
    }

    public void AddThunder(int amount)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddThunder(amount);
            Debug.Log($"⚡ 获得雷神符！当前数量: {SaveManager.Instance.GetThunder()}");
        }
    }

    public bool UseThunder(int amount = 1)
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.UseThunder(amount))
            {
                return true;
            }
            else
            {
                Debug.Log("❌ 雷神符不足！");
                return false;
            }
        }
        return false;
    }
}