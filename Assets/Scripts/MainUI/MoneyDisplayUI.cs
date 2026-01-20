using UnityEngine;
using TMPro;

public class MoneyDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    private bool _isSubscribed = false; // 标记是否已经订阅成功

    void Awake()
    {
        if (coinText == null) coinText = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void Start()
    {
        // 🔥 双重保险：如果 OnEnable 时 Manager 还没醒，Start 时候再试一次
        TrySubscribe();
    }

    void OnDisable()
    {
        if (MoneyManager.Instance != null && _isSubscribed)
        {
            MoneyManager.Instance.OnCoinChanged -= RefreshUI;
            _isSubscribed = false;
        }
    }

    void TrySubscribe()
    {
        // 如果已经订阅过，或者 Manager 还没准备好，就跳过
        if (_isSubscribed || MoneyManager.Instance == null) return;

        // 1. 立刻刷新一次显示
        RefreshUI(MoneyManager.Instance.GetCoins());

        // 2. 订阅事件
        MoneyManager.Instance.OnCoinChanged += RefreshUI;
        _isSubscribed = true;

        Debug.Log("✅ UI 成功连接到 MoneyManager！");
    }

    void RefreshUI(int amount)
    {
        if (coinText != null)
        {
            coinText.text = amount.ToString();
            // Debug.Log($"UI 刷新显示为: {amount}");
        }
    }
}