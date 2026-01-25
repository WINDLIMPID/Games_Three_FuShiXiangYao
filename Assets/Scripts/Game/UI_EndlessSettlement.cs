using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UI_EndlessSettlement : SimpleWindowUI
{
    [Header("=== 文本组件 ===")]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI timeText;

    [Header("=== 视觉组件 ===")]
    public GameObject newRecordBadge;

    [Header("=== 按钮 ===")]
    public Button homeButton;

    public void Show(int finalScore, int bestScore, bool isNewRecord, float secondsPlayed, int killCount)
    {
        if (currentScoreText) currentScoreText.text = finalScore.ToString();
        if (bestScoreText) bestScoreText.text = bestScore.ToString();
        if (killCountText) killCountText.text = killCount.ToString();

        if (timeText)
        {
            int m = Mathf.FloorToInt(secondsPlayed / 60F);
            int s = Mathf.FloorToInt(secondsPlayed % 60F);
            timeText.text = string.Format("{0:00}:{1:00}", m, s);
        }

        if (newRecordBadge) newRecordBadge.SetActive(isNewRecord);

        if (isNewRecord && AudioManager.Instance)
            AudioManager.Instance.PlaySFX("Win");

        base.Show();

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
                // 🔥🔥🔥 核心修改：改用 SceneController 🔥🔥🔥
                if (SceneController.Instance) SceneController.Instance.LoadMainMenu();
                else SceneManager.LoadScene("MainMenuScene");
            });
        }
    }
}