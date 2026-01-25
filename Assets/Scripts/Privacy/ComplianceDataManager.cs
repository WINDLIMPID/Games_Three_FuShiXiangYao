using UnityEngine;
using System.Collections.Generic;
using System;

// 🔥 1. 定义账号等级 (枚举) - 放在类外面，方便全局调用
public enum AccountTier
{
    None,           // 普通玩家 (不应用特殊配置)
    Senior,         // 高级 (解锁全关/Top3/无引导)
    Intermediate,   // 中级 (第6关/3-6名/无引导)
    Junior,         // 低级 (第3关/6名后/无引导)
    Blank           // 空白 (第0关/0分/有引导)
}

// 🔥 2. 定义测试账号结构
[Serializable]
public class TestAccountData
{
    public string username;
    public string password;
    public int age;
    public AccountTier tier; // 账号等级
}

public class ComplianceDataManager : MonoBehaviour
{
    public static ComplianceDataManager Instance;

    [Header("=== 1. 屏蔽词库设置 ===")]
    [Tooltip("请打开屏蔽词文档，全选复制，直接粘贴到这里")]
    [TextArea(10, 20)]
    public string blocklistSourceText;

    // 运行时快速查找的集合
    private HashSet<string> _blockSet = new HashSet<string>();

    [Header("=== 2. 测试账号设置 ===")]
    [Tooltip("右键点击组件名 -> Fill Dev Accounts 可一键填充所有账号")]
    public List<TestAccountData> testAccounts = new List<TestAccountData>();

    void Awake()
    {
        // 保证单例存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 常驻
        }
        else
        {
            Destroy(gameObject);
        }

        InitBlocklist();
    }

    // 初始化屏蔽词库
    void InitBlocklist()
    {
        if (string.IsNullOrEmpty(blocklistSourceText)) return;

        // 支持顿号、逗号、换行符、空格分割
        string[] separators = new string[] { "、", ",", "，", "\n", "\r", " " };
        string[] words = blocklistSourceText.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (var w in words)
        {
            string clean = w.Trim();
            if (!string.IsNullOrEmpty(clean) && !_blockSet.Contains(clean))
            {
                _blockSet.Add(clean);
            }
        }
        Debug.Log($"[合规数据] 屏蔽词库加载完成，共 {_blockSet.Count} 个敏感词。");
    }

    /// <summary>
    /// 检查是否有敏感词
    /// </summary>
    public bool ContainsSensitiveWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        foreach (var word in _blockSet)
        {
            if (input.Contains(word))
            {
                Debug.LogWarning($"[合规] 拦截敏感词: {word}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取测试账号信息
    /// </summary>
    public TestAccountData GetTestAccount(string user, string pwd)
    {
        return testAccounts.Find(a => a.username == user && a.password == pwd);
    }

    // =========================================================
    // 🔥 右键菜单功能：一键填充文档里的所有账号 🔥
    // =========================================================
    [ContextMenu("Fill Dev Accounts (一键填充+分级)")]
    public void FillDevAccounts()
    {
        testAccounts.Clear();
        string pwd = "12345678";

        // --- 1. 高级账号 (Senior) - 成年人 ---
        // 特权：全解锁，分数高，无引导
        AddAcc("admin6", pwd, 25, AccountTier.Senior);
        AddAcc("admin7", pwd, 25, AccountTier.Senior);
        AddAcc("admin8", pwd, 25, AccountTier.Senior);
        AddAcc("admin9", pwd, 25, AccountTier.Senior);

        // --- 2. 中级账号 (Intermediate) ---
        // 特权：解锁到第6关，分数中等
        AddAcc("test0002", pwd, 22, AccountTier.Intermediate);
        AddAcc("test0003", pwd, 22, AccountTier.Intermediate);
        AddAcc("test0004", pwd, 22, AccountTier.Intermediate);
        AddAcc("test0005", pwd, 22, AccountTier.Intermediate);

        // --- 3. 低级账号 (Junior) ---
        // 特权：解锁到第3关，分数低
        AddAcc("test0021", pwd, 22, AccountTier.Junior);
        AddAcc("test0022", pwd, 22, AccountTier.Junior);
        AddAcc("test0023", pwd, 22, AccountTier.Junior);
        AddAcc("test0024", pwd, 22, AccountTier.Junior);

        // --- 4. 空白账号 (Blank) ---
        // 特权：一切归零，开启新手引导
        AddAcc("test0031", pwd, 22, AccountTier.Blank);
        AddAcc("test0032", pwd, 22, AccountTier.Blank);
        AddAcc("test0033", pwd, 22, AccountTier.Blank);
        AddAcc("test0034", pwd, 22, AccountTier.Blank);

        // --- 5. 未成年 (高级/中级/低级/空白 混合) ---
        // 这里根据你的文档逻辑分配：

        // 高级组 (未成年)
        AddAcc("test0101", pwd, 17, AccountTier.Senior); // 16-18岁
        AddAcc("test0102", pwd, 12, AccountTier.Senior); // 8-16岁
        AddAcc("test0103", pwd, 12, AccountTier.Senior); // 8-16岁
        AddAcc("test0301", pwd, 7, AccountTier.Senior); // 8岁以下

        // 中级组 (未成年)
        AddAcc("test0111", pwd, 17, AccountTier.Intermediate);
        AddAcc("test0112", pwd, 12, AccountTier.Intermediate);
        AddAcc("test0302", pwd, 7, AccountTier.Intermediate);

        // 空白组 (未成年)
        AddAcc("test0121", pwd, 17, AccountTier.Blank);

        Debug.Log($"✅ 已自动填入 {testAccounts.Count} 个测试账号，等级配置完毕！");
    }

    void AddAcc(string u, string p, int age, AccountTier tier)
    {
        testAccounts.Add(new TestAccountData
        {
            username = u,
            password = p,
            age = age,
            tier = tier
        });
    }
}