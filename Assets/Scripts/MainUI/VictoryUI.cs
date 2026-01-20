using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 专门负责胜利/结算界面的逻辑控制
/// 符合“三类版号”合规要求：逻辑解耦，UI与战斗分离
/// </summary>
public class VictoryUI : MonoBehaviour
{
    [Header("=== UI 组件引用 ===")]
    [Tooltip("胜利结算的大面板")]
    public GameObject victoryPanel;

    [Tooltip("返回主城/主菜单按钮")]
    public Button backButton;

    [Tooltip("重新开始/再来一局按钮")]
    public Button restartButton;

    [Tooltip("前往下一关/下一境界按钮 (最后一关会自动隐藏)")]
    public Button nextButton;

    // 内部变量：记录当前通关的是第几关
    private int _finishedLevelIndex = 1;

    void Start()
    {
        // 游戏开始时确保面板是隐藏的
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        // 提前绑定好按钮事件
        BindButtons();
    }

    /// <summary>
    /// 核心方法：绑定按钮点击事件
    /// </summary>
    void BindButtons()
    {
        // 1. 返回按钮 (返回道观/主菜单)
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => {
                ResumeGame();
                // 优先使用场景控制器，没有则直接读场景名
                if (SceneController.Instance != null)
                    SceneController.Instance.LoadMainMenu();
                else
                    SceneManager.LoadScene("MainMenuScene");
            });
        }

        // 2. 重玩按钮 (重新修炼)
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => {
                ResumeGame();
                if (SceneController.Instance != null)
                    SceneController.Instance.ReloadCurrentScene();
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }

        // 3. 下一关按钮 (前往下一境界)
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextLevelClicked);
        }
    }

    /// <summary>
    /// 点击“下一关”时的逻辑
    /// </summary>
    void OnNextLevelClicked()
    {
        ResumeGame();

        int nextIndex = _finishedLevelIndex + 1;

        // 更新全局配置
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.currentLevelIndex = nextIndex;

            // 尝试获取下一关的配置数据
            if (GlobalConfig.Instance.levelTable != null &&
                nextIndex <= GlobalConfig.Instance.levelTable.allLevels.Count)
            {
                GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[nextIndex - 1];
            }
        }

        // 切换场景
        if (SceneController.Instance != null)
            SceneController.Instance.LoadBattle();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 🔥 公开方法：供 EnemySpawner 调用，显示胜利界面
    /// </summary>
    /// <param name="levelIndex">当前通关的关卡索引 (1, 2, 3...)</param>
    public void ShowVictory(int levelIndex)
    {
        _finishedLevelIndex = levelIndex;

        // =========================================================
        // 🔥 核心修改：智能判断是否显示“下一关”按钮
        // =========================================================
        bool hasNextLevel = false;

        if (GlobalConfig.Instance != null && GlobalConfig.Instance.levelTable != null)
        {
            // 剧情模式判定：
            // A. levelIndex > 0 (排除掉无尽模式的 -1)
            // B. levelIndex < 总数量 (如果当前是第3关，总共3关，3 < 3 不成立，意味着没有第4关)
            if (levelIndex > 0 && levelIndex < GlobalConfig.Instance.levelTable.allLevels.Count)
            {
                hasNextLevel = true;
            }
        }
        else
        {
            // 如果没有 GlobalConfig (比如直接在场景里测试)，默认显示，方便调试
            // 如果你想严格一点，这里改成 false 也可以
            hasNextLevel = true;
        }

        // 控制按钮显隐
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(hasNextLevel);
        }
        // =========================================================

        // 显示面板并暂停
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // 暂停游戏时间
        }
        else
        {
            Debug.LogError("VictoryUI: 没有绑定 VictoryPanel！");
        }
    }

    // 辅助：恢复时间流速
    void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}