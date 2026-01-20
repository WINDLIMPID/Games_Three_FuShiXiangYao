using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleItemUI : MonoBehaviour
{
    [Header("设置")]
    public Button useButton;       // 拖入按钮自己
    public TextMeshProUGUI countText; // 拖入显示数量的文字

    // 🔥 改动1：将固定数值改为百分比 (0.1 = 10%, 0.5 = 50%)
    [Range(0f, 1f)]
    public float healPercentage = 0.5f; // 默认回 50%

    private void Start()
    {
        // 绑定点击事件
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseItemClicked);
        }

        // 刚进入战斗时刷新一下数量
        RefreshUI();
    }

    private void OnUseItemClicked()
    {
        // 1. 检查道具管家是否存在
        if (ItemManager.Instance == null) return;

        // 2. 尝试消耗 1 个灵芝
        bool isSuccess = ItemManager.Instance.UseLingZhi(1);

        if (isSuccess)
        {
            // 3. 消耗成功，执行回血逻辑
            HealPlayer();

            // 4. 刷新UI数量
            RefreshUI();
        }
        else
        {
            Debug.Log("道具不足，无法使用");
            // 这里可以加个飘字提示 "道具不足"
        }
    }

    // 刷新右下角的数量显示
    private void RefreshUI()
    {
        if (ItemManager.Instance != null && countText != null)
        {
            int count = ItemManager.Instance.GetLingZhiCount();
            countText.text = "x" + count;

            // 如果数量为0，可以让按钮变灰
            if (useButton != null) useButton.interactable = (count > 0);
        }
    }

    // 🔥 改动2：按百分比给主角回血
    private void HealPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Health healthScript = player.GetComponent<Health>();

            if (healthScript != null)
            {
                // 🔥 计算实际回血量：上限 * 百分比
                int amount = Mathf.RoundToInt(healthScript.maxHealth * healPercentage);

                // 至少回1点血，防止算出来是0
                if (amount < 1) amount = 1;

                healthScript.Heal(amount);

                Debug.Log($"💊 使用灵芝！上限: {healthScript.maxHealth}, 恢复({healPercentage * 100}%): {amount}");

                // 播放个音效
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("Heal"); // 记得在AudioManager里配一个叫 "Heal" 的音效
            }
            else
            {
                Debug.LogError("❌ 找到了主角物体，但它身上没有挂 Health 脚本！");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 场景里没找到 Tag 为 Player 的物体，无法回血！");
        }
    }
}