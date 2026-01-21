using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UI_EndlessSettlement : SimpleWindowUI
{
    [Header("=== 文本组件 (请按顺序拖入) ===")]
    public TextMeshProUGUI currentScoreText; // 第一行：显示计算后的【总分】
    public TextMeshProUGUI bestScoreText;    // 第二行：显示【最高分】
    public TextMeshProUGUI killCountText;    // 第三行：显示【降妖数】(原 rewardText)
    public TextMeshProUGUI timeText;         // 第四行：显示【用时】

    [Header("=== 视觉组件 ===")]
    public GameObject newRecordBadge;        // 新纪录印章

    [Header("=== 按钮 ===")]
    public Button homeButton;

    /// <summary>
    /// 显示结算面板
    /// </summary>
    /// <param name="finalScore">计算后的总分</param>
    /// <param name="bestScore">历史最高分</param>
    /// <param name="isNewRecord">是否新纪录</param>
    /// <param name="secondsPlayed">生存时间(秒)</param>
    /// <param name="killCount">击杀数量</param>
    public void Show(int finalScore, int bestScore, bool isNewRecord, float secondsPlayed, int killCount)
    {
        // 1. 设置文本
        if (currentScoreText) currentScoreText.text = finalScore.ToString();
        if (bestScoreText) bestScoreText.text = bestScore.ToString();

        // 第三行显示降妖数 (击杀数)
        if (killCountText) killCountText.text = killCount.ToString();

        // 格式化时间
        if (timeText)
        {
            int m = Mathf.FloorToInt(secondsPlayed / 60F);
            int s = Mathf.FloorToInt(secondsPlayed % 60F);
            timeText.text = string.Format("{0:00}:{1:00}", m, s);
        }

        // 2. 新纪录显示
        if (newRecordBadge) newRecordBadge.SetActive(isNewRecord);

        // 3. 音效
        if (isNewRecord && AudioManager.Instance)
            AudioManager.Instance.PlaySFX("Win");

        // 4. 显示弹窗
        base.Show();

        // 5. 分数滚动动画
        if (currentScoreText)
        {
            int tempScore = 0;
            DOTween.To(() => tempScore, x => tempScore = x, finalScore, 1.0f)
                .OnUpdate(() => currentScoreText.text = tempScore.ToString())
                .SetUpdate(true);
        }
    }

    void Start()
    {
        if (homeButton)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => {
                Time.timeScale = 1f;
                if (SceneController.Instance) SceneController.Instance.LoadMainMenu();
                else SceneManager.LoadScene("MainMenuScene");
            });
        }
    }
}