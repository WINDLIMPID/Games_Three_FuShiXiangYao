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
            // 直接调用父类的 Hide 方法 (带动画关闭)
            closeButton.onClick.AddListener(Hide);
        }
        
    }

    // 重写父类的 Show 方法
    public override void Show()
    {
        // 1. 先刷新数据 (确保弹出来的时候数据是最新的)
        RefreshUI();

        // 2. 再调用父类的 Show (播放果冻弹窗动画)
        base.Show();

        // (可选) 如果你想打开排行榜时暂停游戏，取消下面这行的注释
        // Time.timeScale = 0f; 
    }

    // (可选) 重写父类的关闭回调
    protected override void OnHideComplete()
    {
        base.OnHideComplete();
        // 如果你上面暂停了游戏，这里记得恢复
        // Time.timeScale = 1f;
    }

    void RefreshUI()
    {
        // --- 这部分逻辑保持不变 ---

        // 1. 清理旧列表
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 获取数据
        if (LeaderboardSystem.Instance == null) return;
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