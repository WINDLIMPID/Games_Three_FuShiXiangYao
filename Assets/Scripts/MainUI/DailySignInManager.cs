using UnityEngine;
using UnityEngine.UI;
using TMPro; // 🔥 必须引用这个，因为你要改文字
using System;

// 继承 SimpleWindowUI (保持你刚才的改动)
public class DailySignInManager : SimpleWindowUI
{
    [Header("=== 签到组件 (子类特有) ===")]
    public Transform daysParent;
    public Button signInButton;

    // 🔥 新增：用来显示顶部大金币数量的文字
    public TextMeshProUGUI todayRewardText;

    [Header("=== 配置 ===")]
    public int totalDays = 7;
    public int[] dailyRewards = new int[7] { 100, 200, 300, 500, 600, 800, 1000 };

    private const string PREF_LAST_DATE = "SignIn_LastDate";
    private const string PREF_DAY_INDEX = "SignIn_DayIndex";

    void Start()
    {
        if (signInButton != null)
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

    public void RefreshUI()
    {
        int currentDayIndex = PlayerPrefs.GetInt(PREF_DAY_INDEX, 0);
        bool isSignedToday = CheckIfSignedToday();

        // 1. 自动重置逻辑 (新的一周)
        if (currentDayIndex >= totalDays && !isSignedToday)
        {
            currentDayIndex = 0;
            PlayerPrefs.SetInt(PREF_DAY_INDEX, 0);
            PlayerPrefs.Save();
        }

        // 🔥 2. 更新顶部大金币的文字显示
        if (todayRewardText != null)
        {
            // 防止数组越界（比如配置了7天但代码跑到第8天）
            int safeIndex = Mathf.Clamp(currentDayIndex, 0, dailyRewards.Length - 1);

            // 如果今天签过了，currentDayIndex 其实已经指向明天了，
            // 这种情况下显示“明天的奖励”作为预告是非常好的体验。
            // 或者你想显示刚才拿到的钱？通常显示明天的预告比较多。
            // 这里我们默认显示 currentDayIndex 对应的金额（即：未签到时显示今天的，已签到时显示明天的）。

            int amount = dailyRewards[safeIndex];
            todayRewardText.text = amount.ToString();
        }

        // 3. 刷新 Grid 里的勾勾
        for (int i = 0; i < totalDays; i++)
        {
            if (i >= daysParent.childCount) break;

            Transform dayNode = daysParent.GetChild(i);
            Transform rightMark = dayNode.Find("Right");

            if (rightMark != null)
            {
                bool showCheck = i < currentDayIndex;
                rightMark.gameObject.SetActive(showCheck);
            }
        }

        // 4. 按钮状态
        if (signInButton != null)
        {
            if (currentDayIndex >= totalDays)
                signInButton.interactable = false;
            else
                signInButton.interactable = !isSignedToday;
        }
    }

    void OnSignInClicked()
    {
        int currentDayIndex = PlayerPrefs.GetInt(PREF_DAY_INDEX, 0);
        if (currentDayIndex >= totalDays) return;

        // 获取奖励
        int rewardAmount = 100;
        if (dailyRewards != null && currentDayIndex < dailyRewards.Length)
        {
            rewardAmount = dailyRewards[currentDayIndex];
        }

        // 发钱
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddCoins(rewardAmount);
        }

        // 记录日期 & 进度
        PlayerPrefs.SetString(PREF_LAST_DATE, DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt(PREF_DAY_INDEX, currentDayIndex + 1);
        PlayerPrefs.Save();

        RefreshUI();
    }

    bool CheckIfSignedToday()
    {
        string lastDateStr = PlayerPrefs.GetString(PREF_LAST_DATE, "");
        if (string.IsNullOrEmpty(lastDateStr)) return false;
        DateTime lastDate = DateTime.Parse(lastDateStr);
        DateTime today = DateTime.Now;
        return lastDate.Year == today.Year && lastDate.Month == today.Month && lastDate.Day == today.Day;
    }

    // --- 测试功能保持不变 ---
    [ContextMenu("🗑️ 重置签到数据")]
    public void Test_ResetData() { /* ...同前... */ PlayerPrefs.DeleteKey(PREF_LAST_DATE); PlayerPrefs.DeleteKey(PREF_DAY_INDEX); RefreshUI(); }
    [ContextMenu("⏭️ 模拟进入第二天")]
    public void Test_SimulateNextDay() { /* ...同前... */ PlayerPrefs.SetString(PREF_LAST_DATE, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")); RefreshUI(); }
}