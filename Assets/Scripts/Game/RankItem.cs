using UnityEngine;
using TMPro; // 引用 TextMeshPro

public class RankItem : MonoBehaviour
{
    [Header("背景控制 (拖入对应的子物体)")]
    public GameObject normalBG;   // 普通背景（暗的）
    public GameObject playerBG;   // 玩家背景（亮的/高亮的）

    [Header("文本组件")]
    public TextMeshProUGUI rankText;  // 排名
    public TextMeshProUGUI nameText;  // 名字
    public TextMeshProUGUI scoreText; // 积分

    // 公开方法：供 UI_Leaderboard 调用
    public void Setup(int rank, string name, int score, bool isPlayer)
    {
        // 1. 切换背景状态
        if (normalBG != null) normalBG.SetActive(!isPlayer); // 不是玩家 -> 开普通
        if (playerBG != null) playerBG.SetActive(isPlayer);  // 是玩家 -> 开高亮

        // 2. 填充文本
        if (rankText != null)
        {
            rankText.text = rank.ToString();
            // 前三名颜色特殊处理 (可选)
            if (rank == 1) rankText.color = new Color32(140, 37, 0, 255); // 金
            else if (rank == 2) rankText.color = new Color32(183, 67, 0, 255); // 银
            else if (rank == 3) rankText.color = new Color32(229, 115, 0, 255); // 铜
            
        }

        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = score.ToString();
    }
}