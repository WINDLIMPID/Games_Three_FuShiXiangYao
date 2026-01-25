using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopManager : SimpleWindowUI
{
    [Header("商品")]
    public Button buyItemBtn;
    public int itemPrice = 50;

    public Button freeGiftBtn;
    public TextMeshProUGUI timerText;
    public int freeRewardAmount = 100;

    public Button adGiftBtn;
    public int adRewardAmount = 200;

    public Button buyThunderBtn;
    public int thunderPrice = 500;

    [Header("提示")]
    public GameObject toastPanel;
    public TextMeshProUGUI toastText;

    private const string PREF_LAST_FREE_TIME = "Shop_LastFreeTime";
    private const double COOLDOWN_HOURS = 24.0;

    void Start()
    {
        if (buyItemBtn) buyItemBtn.onClick.AddListener(OnBuyItemClicked);
        if (freeGiftBtn) freeGiftBtn.onClick.AddListener(OnFreeGiftClicked);
        if (adGiftBtn) adGiftBtn.onClick.AddListener(OnWatchAdClicked);
        if (buyThunderBtn) buyThunderBtn.onClick.AddListener(OnBuyThunderClicked);
        HideTip();
    }

    void Update() => UpdateFreeTimer();

    void OnBuyItemClicked()
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.SpendMoney(itemPrice))
            {
                if (ItemManager.Instance) ItemManager.Instance.AddLingZhi(1);
                ShowTip("购买成功");
            }
            else ShowTip("金币不足");
        }
    }

    void OnBuyThunderClicked()
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.SpendMoney(thunderPrice))
            {
                if (ItemManager.Instance) ItemManager.Instance.AddThunder(1);
                ShowTip("购买成功");
            }
            else ShowTip("金币不足");
        }
    }

    void OnWatchAdClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddMoney(adRewardAmount);
            ShowTip("金元宝+" + adRewardAmount);
        }
    }

    void OnFreeGiftClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddMoney(freeRewardAmount);
            ShowTip("金元宝+" + freeRewardAmount);
        }
        PlayerPrefs.SetString(PREF_LAST_FREE_TIME, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    void ShowTip(string message) { if (toastText) toastText.text = message; if (toastPanel) { toastPanel.SetActive(true); CancelInvoke("HideTip"); Invoke("HideTip", 2f); } }
    void HideTip() { if (toastPanel) toastPanel.SetActive(false); }

    void UpdateFreeTimer()
    {
        if (timerText == null || freeGiftBtn == null) return;
        string lastTimeStr = PlayerPrefs.GetString(PREF_LAST_FREE_TIME, "");
        if (string.IsNullOrEmpty(lastTimeStr)) { EnableFreeButton(true); return; }

        DateTime lastTime = DateTime.Parse(lastTimeStr);
        double hoursLeft = COOLDOWN_HOURS - (DateTime.Now - lastTime).TotalHours;

        if (hoursLeft <= 0) EnableFreeButton(true);
        else
        {
            freeGiftBtn.interactable = false;
            TimeSpan t = TimeSpan.FromHours(hoursLeft);
            timerText.text = $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }
    void EnableFreeButton(bool enable) { freeGiftBtn.interactable = enable; if (enable) timerText.text = "免费领取"; }
}