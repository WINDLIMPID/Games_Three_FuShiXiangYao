using UnityEngine;
using TMPro;

public class MoneyDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    private bool _isSubscribed = false;

    void Awake()
    {
        if (coinText == null) coinText = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable() => TrySubscribe();
    void Start() => TrySubscribe();

    void OnDisable()
    {
        if (SaveManager.Instance != null && _isSubscribed)
        {
            SaveManager.Instance.OnCoinChanged -= RefreshUI;
            _isSubscribed = false;
        }
    }

    void TrySubscribe()
    {
        if (SaveManager.Instance == null || _isSubscribed) return;

        RefreshUI(SaveManager.Instance.GetCoin());
        SaveManager.Instance.OnCoinChanged += RefreshUI;
        _isSubscribed = true;
    }

    void RefreshUI(int amount)
    {
        if (coinText != null) coinText.text = amount.ToString();
    }
}