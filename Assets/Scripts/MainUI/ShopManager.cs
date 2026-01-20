using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 继承自 SimpleWindowUI
public class ShopManager : SimpleWindowUI
{
    [Header("=== 商品 1：千年灵芝 (金币购买) ===")]
    public Button buyItemBtn;
    public int itemPrice = 50;

    [Header("=== 商品 2：金元宝 (24小时免费) ===")]
    public Button freeGiftBtn;
    public TextMeshProUGUI timerText;
    public int freeRewardAmount = 100;

    // 内部配置：免费领取倒计时
    private const string PREF_LAST_FREE_TIME = "Shop_LastFreeTime";
    private const double COOLDOWN_HOURS = 24.0;

    [Header("=== 商品 3：金元宝 (看广告) ===")]
    public Button adGiftBtn;
    public int adRewardAmount = 200;

    [Header("=== 🔥 新增商品 4：九天雷神符 ===")]
    public Button buyThunderBtn;   // ⚡ 请在 Inspector 里拖入新的购买按钮
    public int thunderPrice = 500; // ⚡ 价格

    [Header("=== 提示框设置 (Toast) ===")]
    public GameObject toastPanel;
    public TextMeshProUGUI toastText;

    void Start()
    {
        // 1. 绑定灵芝购买
        if (buyItemBtn != null)
            buyItemBtn.onClick.AddListener(OnBuyItemClicked);

        // 2. 绑定免费领取
        if (freeGiftBtn != null)
            freeGiftBtn.onClick.AddListener(OnFreeGiftClicked);

        // 3. 绑定广告
        if (adGiftBtn != null)
            adGiftBtn.onClick.AddListener(OnWatchAdClicked);

        // 4. 🔥 绑定雷符购买
        if (buyThunderBtn != null)
            buyThunderBtn.onClick.AddListener(OnBuyThunderClicked);

        HideTip();
    }

    void Update()
    {
        UpdateFreeTimer();
    }

    // ==========================================
    // 购买逻辑
    // ==========================================

    void OnBuyItemClicked()
    {
        if (MoneyManager.Instance != null)
        {
            if (MoneyManager.Instance.SpendCoins(itemPrice))
            {
                if (ItemManager.Instance != null) ItemManager.Instance.AddLingZhi(1);
                ShowTip("购买灵芝成功");
            }
            else
            {
                ShowTip("金币不足！");
            }
        }
    }

    // 🔥🔥🔥 新增：购买雷符 🔥🔥🔥
    void OnBuyThunderClicked()
    {
        if (MoneyManager.Instance != null)
        {
            // 尝试扣款
            bool success = MoneyManager.Instance.SpendCoins(thunderPrice);

            if (success)
            {
                // 加货
                if (ItemManager.Instance != null)
                {
                    ItemManager.Instance.AddThunder(1);
                }
                ShowTip("获得雷神符");
            }
            else
            {
                ShowTip("金币不足！需要 " + thunderPrice);
            }
        }
    }

    // ==========================================
    // 辅助功能 (提示框 & 倒计时)
    // ==========================================
    void ShowTip(string message)
    {
        if (toastText != null) toastText.text = message;
        if (toastPanel != null)
        {
            toastPanel.SetActive(true);
            CancelInvoke("HideTip");
            Invoke("HideTip", 2.0f);
        }
    }

    void HideTip()
    {
        if (toastPanel != null) toastPanel.SetActive(false);
    }

    void OnFreeGiftClicked()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddCoins(freeRewardAmount);
            ShowTip("金元宝+" + freeRewardAmount);
        }
        PlayerPrefs.SetString(PREF_LAST_FREE_TIME, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    void UpdateFreeTimer()
    {
        if (timerText == null || freeGiftBtn == null) return;
        string lastTimeStr = PlayerPrefs.GetString(PREF_LAST_FREE_TIME, "");

        if (string.IsNullOrEmpty(lastTimeStr))
        {
            EnableFreeButton(true);
            return;
        }

        DateTime lastTime = DateTime.Parse(lastTimeStr);
        TimeSpan diff = DateTime.Now - lastTime;
        double hoursLeft = COOLDOWN_HOURS - diff.TotalHours;

        if (hoursLeft <= 0)
        {
            EnableFreeButton(true);
        }
        else
        {
            freeGiftBtn.interactable = false;
            TimeSpan timeLeft = TimeSpan.FromHours(hoursLeft);
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
        }
    }

    void EnableFreeButton(bool enable)
    {
        freeGiftBtn.interactable = enable;
        if (enable) timerText.text = "免费领取";
    }

    void OnWatchAdClicked()
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddCoins(adRewardAmount);
            ShowTip("金元宝+" + adRewardAmount);
        }
    }
}