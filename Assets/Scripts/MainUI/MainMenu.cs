using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI 界面引用")]
    public GameObject chooseLevelUI;
    public GameObject roleUI; // 角色界面 (RoleUI)
    public GameObject mainUI; // 主界面 (MainUI)

    [Header("🔥 无尽模式弹窗引用 (请拖拽)")]
    public GameObject endlessPanel;   // 无尽模式的那个弹窗物体
    public Button endlessStartButton; // 弹窗里那个“开始挑战”的按钮
    public Button endlessCloseButton; // 弹窗右上角的“X”关闭按钮

    [Header("🔥 漫画播放器")]
    public StoryPlayerUI storyPlayer;

    [Header("功能模块")]
    public DailySignInManager signInManager;
    public GameObject realNamePanel;
    public UI_LoginPanel loginPanel;

    [Header("无尽模式配置")]
    public LevelConfigEntry endlessLevelConfig;

    // Key 前缀
    private const string PREF_STORY_WATCHED_PREFIX = "HasWatchedIntroStory_";

    void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic("UIBGM");

        // 🔥 自动绑定无尽模式按钮事件
        if (endlessStartButton != null)
        {
            endlessStartButton.onClick.RemoveAllListeners();
            endlessStartButton.onClick.AddListener(StartEndlessChallenge);
        }

        if (endlessCloseButton != null)
        {
            endlessCloseButton.onClick.RemoveAllListeners();
            endlessCloseButton.onClick.AddListener(CloseEndlessPanel);
        }

        // 确保一开始弹窗是关的
        if (endlessPanel != null) endlessPanel.SetActive(false);

        CheckLoginStatus();
    }

    // ... (中间的登录、实名、漫画逻辑保持不变，省略以节省篇幅) ...
    // ... 请保留 CheckLoginStatus, CheckVerificationFlow, InitGameFlow, StartRoleUI 等方法 ...
    // 下面只列出修改过的部分和必要的辅助方法

    void CheckLoginStatus()
    {
        if (AccountManager.Instance != null && !AccountManager.Instance.isLoggedIn)
        {
            if (loginPanel != null) loginPanel.Show();
        }
        else
        {
            CheckVerificationFlow();
        }
    }

    public void CheckVerificationFlow()
    {
        if (AntiAddictionManager.Instance != null && !AntiAddictionManager.Instance.isVerified)
        {
            if (realNamePanel != null) realNamePanel.SetActive(true);
        }
        else
        {
            InitGameFlow();
        }
    }

    public void InitGameFlow()
    {
        if (GlobalConfig.Instance != null && GlobalConfig.Instance.isLevelSelectionOpen)
        {
            StartChooseLevel(true);
        }
        else
        {
            StartMainUI(true);
            StartRoleUI(false);
            StartChooseLevel(false);
        }
    }

    public void StartRoleUI(bool _isOpen)
    {
        // ... (保持原有的漫画播放逻辑不变) ...
        if (_isOpen && storyPlayer != null)
        {
            string username = "default";
            if (AccountManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(AccountManager.Instance.currentUsername))
                    username = AccountManager.Instance.currentUsername;
                else
                    username = AccountManager.Instance.GetLastUsedUsername();
            }

            string key = PREF_STORY_WATCHED_PREFIX + username;
            bool hasWatched = PlayerPrefs.GetInt(key, 0) == 1;

            if (!hasWatched)
            {
                if (mainUI) mainUI.SetActive(false);
                storyPlayer.PlayStory(() => {
                    PlayerPrefs.SetInt(key, 1);
                    PlayerPrefs.Save();
                    if (roleUI != null) roleUI.SetActive(true);
                });
                return;
            }
        }
        if (roleUI != null) roleUI.SetActive(_isOpen);
    }

    public void StartMainUI(bool _isOpen)
    {
        if (GlobalConfig.Instance) GlobalConfig.Instance.isLevelSelectionOpen = _isOpen;
        if (mainUI) mainUI.SetActive(_isOpen);
    }

    public void StartChooseLevel(bool _isOpen)
    {
        if (GlobalConfig.Instance) GlobalConfig.Instance.isLevelSelectionOpen = _isOpen;
        if (chooseLevelUI) chooseLevelUI.SetActive(_isOpen);
    }

    public void StartStoryLevel(int levelIndex)
    {
        if (GlobalConfig.Instance == null || GlobalConfig.Instance.levelTable == null) return;

        GlobalConfig.Instance.currentLevelIndex = levelIndex;
        if (levelIndex >= 0 && levelIndex < GlobalConfig.Instance.levelTable.allLevels.Count)
        {
            GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[levelIndex];
            EnterBattleScene();
        }
    }

    // ==========================================
    // 🔥 无尽模式核心逻辑
    // ==========================================

    // 打开无尽模式弹窗 (绑定给主界面的那个"无尽模式"入口按钮)
    public void OpenEndlessPanel()
    {
        if (endlessPanel != null) endlessPanel.SetActive(true);
    }

    // 关闭无尽模式弹窗
    public void CloseEndlessPanel()
    {
        if (endlessPanel != null) endlessPanel.SetActive(false);
    }

    // 点击"开始挑战"时执行
    public void StartEndlessChallenge()
    {
        Debug.Log("🚀 进入无尽模式！");

        // 1. 设置关卡索引为 -1 (代表无尽)
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.currentLevelIndex = -1;

            // 2. 确保配置存在
            if (endlessLevelConfig == null)
            {
                endlessLevelConfig = new LevelConfigEntry()
                {
                    displayTitle = "无尽荒原",
                    surviveDuration = 99999f, // 时间无限
                    spawnPointGroupName = "Map1Point" // 确保这里填了有效的刷怪点名字
                };
            }
            GlobalConfig.Instance.currentLevelConfig = endlessLevelConfig;
        }

        // 3. 进入战斗
        EnterBattleScene();
    }

    private void EnterBattleScene()
    {
        // 优先使用 SceneController，没有则直接加载
        if (SceneController.Instance != null) SceneController.Instance.LoadBattle();
        else SceneManager.LoadScene("BattleScene");
    }

    public void OpenSignInPanel() { if (signInManager) signInManager.Show(); }
    public void OnReplayStoryClicked() { if (storyPlayer != null) storyPlayer.PlayStory(() => { }); }
    public void QuitGame() { Application.Quit(); }
}