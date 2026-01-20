using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button homneButton;

    private bool _hasBoundPlayer = false;

    // 🔥 新增：保存玩家 Health 脚本的引用，用于检查是否复活
    private Health _playerHealth;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (homneButton != null)
        {
            homneButton.onClick.AddListener(ReTurnHome);
        }
    }

    void Update()
    {
        if (!_hasBoundPlayer)
        {
            FindAndBindPlayer();
        }
    }

    void FindAndBindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null)
            {
                // 🔥 记录引用
                _playerHealth = hp;

                hp.OnDeath -= ShowGameOver;
                hp.OnDeath += ShowGameOver;

                _hasBoundPlayer = true;
            }
        }
    }

    public void ReTurnHome()
    {
        Time.timeScale = 1f;
        SceneController.Instance.LoadMainMenu();
    }

    void ShowGameOver()
    {
        // 收到死亡消息，1.5秒后准备弹窗
        Invoke("EnablePanel", 1.5f);
    }

    void EnablePanel()
    {
        // 🔥🔥🔥 核心修复在这里！🔥🔥🔥
        // 在弹出界面前，最后检查一次：玩家真的还死着吗？
        // 如果玩家已经复活了 (isDead == false)，就取消这次弹窗！
        if (_playerHealth != null && !_playerHealth.isDead)
        {
            Debug.Log("⚠️ 检测到玩家已复活，取消显示失败界面！");
            return;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneController.Instance.ReloadCurrentScene();
    }
}