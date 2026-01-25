using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_Leaderboard : SimpleWindowUI
{
    [Header("=== 排行榜组件 ===")]
    public Transform contentParent; // 滚动列表的 Content
    public GameObject itemPrefab;   // 单条排名的预制体
    public Button closeButton;      // 关闭按钮

    void Start()
    {
        // 绑定关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
    }

    // 重写父类的 Show 方法
    public override void Show()
    {
        // 1. 🔥🔥🔥 核心修复：打开界面时，强制让系统刷新数据！🔥🔥🔥
        // 这样能保证每次打开看到的都是最新的分数（从SaveManager读取的）
        if (LeaderboardSystem.Instance != null)
        {
            LeaderboardSystem.Instance.RefreshLeaderboard();
        }

        // 2. 刷新 UI 显示
        RefreshUI();

        // 3. 再调用父类的 Show (播放果冻弹窗动画)
        base.Show();
    }

    void RefreshUI()
    {
        // 1. 清理旧列表
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 获取数据
        if (LeaderboardSystem.Instance == null) return;

        // 获取刚刚刷新过的最新数据
        var dataList = LeaderboardSystem.Instance.GetLeaderboardData();

        // 3. 生成列表
        foreach (var data in dataList)
        {
            GameObject obj = Instantiate(itemPrefab, contentParent);

            RankItem itemScript = obj.GetComponent<RankItem>();
            if (itemScript != null)
            {
                itemScript.Setup(data.rank, data.name, data.score, data.isPlayer);
            }
        }
    }
}