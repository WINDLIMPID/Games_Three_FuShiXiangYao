using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailySignInManager : SimpleWindowUI
{
    [Header("=== 签到组件 ===")]
    public Transform daysParent;
    public Button signInButton;
    public TextMeshProUGUI todayRewardText;

    [Header("=== 配置 ===")]
    public int totalDays = 7;
    public int[] dailyRewards = new int[7] { 100, 200, 300, 500, 600, 800, 1000 };

    // 基础 Key，后面会自动拼接用户名
    private const string PREF_LAST_DATE_BASE = "SignIn_LastDate";
    private const string PREF_DAY_INDEX_BASE = "SignIn_DayIndex";

    void Start()
    {
        if (signInButton)
        {
            signInButton.onClick.RemoveAllListeners();
            signInButton.onClick.AddListener(OnSignInClicked);
        }
        RefreshUI();
    }

    public override void Show()
    {
        base.Show();
        RefreshUI();
    }

    // 🔥🔥🔥 核心修复：获取带用户名的专属 Key 🔥🔥🔥
    private string GetUserKey(string baseKey)
    {
        string username = "default";

        // 尝试从 AccountManager 获取当前用户名
        if (AccountManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(AccountManager.Instance.currentUsername))
            {
                username = AccountManager.Instance.currentUsername;
            }
            else
            {
                username = AccountManager.Instance.GetLastUsedUsername();
            }
        }

        // 返回类似于 "SignIn_DayIndex_TestUser01" 的 Key
        return baseKey + "_" + username;
    }

    public void RefreshUI()
    {
        // 🔥 使用 GetUserKey 读取数据
        string keyIndex = GetUserKey(PREF_DAY_INDEX_BASE);
        string keyDate = GetUserKey(PREF_LAST_DATE_BASE);

        int currentDayIndex = PlayerPrefs.GetInt(keyIndex, 0);
        bool isSignedToday = CheckIfSignedToday(keyDate);

        // 自动重置新的一周
        if (currentDayIndex >= totalDays && !isSignedToday)
        {
            currentDayIndex = 0;
            PlayerPrefs.SetInt(keyIndex, 0);
            PlayerPrefs.Save();
        }

        // 显示金额文字
        if (todayRewardText != null)
        {
            int safeIndex = Mathf.Clamp(currentDayIndex, 0, dailyRewards.Length - 1);
            todayRewardText.text = dailyRewards[safeIndex].ToString();
        }

        // 刷新勾勾
        for (int i = 0; i < totalDays; i++)
        {
            if (i >= daysParent.childCount) break;
            Transform rightMark = daysParent.GetChild(i).Find("Right");
            if (rightMark) rightMark.gameObject.SetActive(i < currentDayIndex);
        }

        if (signInButton) signInButton.interactable = (currentDayIndex < totalDays) && !isSignedToday;
    }

    void OnSignInClicked()
    {
        string keyIndex = GetUserKey(PREF_DAY_INDEX_BASE);
        string keyDate = GetUserKey(PREF_LAST_DATE_BASE);

        int currentDayIndex = PlayerPrefs.GetInt(keyIndex, 0);
        if (currentDayIndex >= totalDays) return;

        int rewardAmount = (currentDayIndex < dailyRewards.Length) ? dailyRewards[currentDayIndex] : 100;

        // 发钱
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddMoney(rewardAmount);
            Debug.Log($"签到成功，发放 {rewardAmount} 金币");
        }

        // 🔥 使用 GetUserKey 保存数据
        PlayerPrefs.SetString(keyDate, DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(keyIndex, currentDayIndex + 1);
        PlayerPrefs.Save();

        RefreshUI();
    }

    bool CheckIfSignedToday(string keyDate)
    {
        string lastDateStr = PlayerPrefs.GetString(keyDate, "");
        if (string.IsNullOrEmpty(lastDateStr)) return false;

        DateTime lastDate;
        if (DateTime.TryParse(lastDateStr, out lastDate))
        {
            DateTime today = DateTime.Now;
            return lastDate.Year == today.Year && lastDate.Month == today.Month && lastDate.Day == today.Day;
        }
        return false;
    }
}