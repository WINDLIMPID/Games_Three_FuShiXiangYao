using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI 界面引用")]
    public GameObject chooseLevelUI;
    public GameObject roleUI;

    public GameObject mainUI;

    [Header("🔥 新增：漫画播放器")]
    public StoryPlayerUI storyPlayer;

    [Header("功能模块")]
    public DailySignInManager signInManager; // 签到管理器
    public GameObject realNamePanel;         // 实名认证弹窗
    public UI_LoginPanel loginPanel;         // 登录弹窗

    [Header("🔥 无尽模式配置 (请在 Inspector 设置)")]
    // 核心修改：这里允许你在 Unity 编辑器里直接配置无尽关卡的数值，不用改代码！
    // 建议把 Title 设为 "无尽试炼"，时间设为 999999
    public LevelConfigEntry endlessLevelConfig;


    private const string PREF_STORY_WATCHED = "HasWatchedIntroStory"; // 存档Key

    void Start()
    {
        // 1. 播放 UI BGM
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic("UIBGM");
        }

        // 2. 检查登录状态
        CheckLoginStatus();

        // 🔥🔥🔥 核心修改：根据全局状态决定显示哪个界面
        if (GlobalConfig.Instance != null)
        {
            // 如果记录为打开状态，直接进选关界面
            if (GlobalConfig.Instance.isLevelSelectionOpen)
            {
                StartChooseLevel(true);
            }
            else
            {
                // 否则显示主界面 (重置状态)
                StartMainUI(true);
                StartRoleUI(false);
                StartChooseLevel(false);
            }
        }
    }

    void CheckLoginStatus()
    {
        // 只要当前内存里没有登录状态，就强制弹窗
        if (AccountManager.Instance != null && !AccountManager.Instance.isLoggedIn)
        {
            if (loginPanel != null)
            {
                loginPanel.Show();
            }
        }
        else
        {
            // 如果已经登录了，检查实名认证流程
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
            // 🔥 实名已通过 -> 进入正常的初始化流程
            InitGameFlow();
        }
    }

    // =========================================================
    // 🔥 修改点 1：移除开场播放逻辑，直接进主界面
    // =========================================================
    public void InitGameFlow()
    {
        // 旧逻辑已删除，不再这里判断是否看过漫画
        // 直接显示正常的主界面
        ShowNormalUI();
    }

    // 显示正常的主界面逻辑
    void ShowNormalUI()
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

    // 🔥 手动点击“回看漫画”按钮
    public void OnReplayStoryClicked()
    {
        if (storyPlayer != null)
        {
            // 这里的回调是空的，或者只是单纯关闭，不需要进游戏
            storyPlayer.PlayStory(() => {
                // 播完啥也不用做，StoryPlayerUI 会自己把自己关掉
                Debug.Log("重温结束");
            });
        }
    }

    // =========================================================
    // 🔥 核心修改区域：区分剧情模式和无尽模式
    // =========================================================

    /// <summary>
    /// 开始剧情模式 (请把选关界面 LevelButton 的点击事件绑定到这里)
    /// </summary>
    /// <param name="levelIndex">关卡索引 (0=第一关, 1=第二关...)</param>
    public void StartStoryLevel(int levelIndex)
    {
        if (!IsConfigValid()) return;

        // 1. 设置标记：这是剧情关卡 (索引 >= 0)
        GlobalConfig.Instance.currentLevelIndex = levelIndex;

        // 2. 校验索引防止越界
        if (levelIndex < 0 || levelIndex >= GlobalConfig.Instance.levelTable.allLevels.Count)
        {
            Debug.LogError($"❌ 关卡索引 {levelIndex} 超出配置表范围！请检查 GameLevelTable Asset 文件。");
            return;
        }

        // 3. 从总表读取配置
        GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[levelIndex];

        Debug.Log($"🚀 进入剧情第 {levelIndex + 1} 关: {GlobalConfig.Instance.currentLevelConfig.displayTitle}");
        EnterBattleScene();
    }

    /// <summary>
    /// 开始无尽挑战 (请把主界面“无尽挑战”按钮的 OnClick 绑定到这里)
    /// </summary>
    public void StartEndlessChallenge()
    {
        // 1. 设置标记：-1 代表特殊模式/无尽模式
        // (这样结算界面看到 -1，就知道不要显示“下一关”按钮了)
        GlobalConfig.Instance.currentLevelIndex = -1;

        // 2. 🔥 使用 Inspector 里配置的独立数据！
        if (endlessLevelConfig == null)
        {
            Debug.LogError("❌ 你还没在 MainMenu 的 Inspector 里配置 Endless Level Config！请快去填数据！");
            // 临时生成一个保底数据，防止报错卡死
            endlessLevelConfig = new LevelConfigEntry() { displayTitle = "临时无尽", surviveDuration = 9999f };
        }

        // (安全保底) 确保时间足够长
        if (endlessLevelConfig.surviveDuration < 9999f)
        {
            endlessLevelConfig.surviveDuration = 999999f;
        }

        GlobalConfig.Instance.currentLevelConfig = endlessLevelConfig;

        Debug.Log($"🚀 进入无尽挑战模式: {endlessLevelConfig.displayTitle}");
        EnterBattleScene();
    }

    /// <summary>
    /// 旧的开始按钮逻辑 (为了兼容你现有的“新游戏”按钮)
    /// </summary>
    public void StartGame()
    {
        // 默认进入剧情第1关 (索引0)
        // 如果你想让“新游戏”按钮直接进无尽，改成调用 StartEndlessChallenge() 即可
        //StartStoryLevel(0);
        StartEndlessChallenge();
    }

    // 统一的场景加载入口
    private void EnterBattleScene()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadBattle();
        }
        else
        {
            // 备用方案
            SceneManager.LoadScene("BattleScene");
        }
    }

    private bool IsConfigValid()
    {
        if (GlobalConfig.Instance == null)
        {
            Debug.LogError("❌ GlobalConfig 未初始化！");
            return false;
        }
        if (GlobalConfig.Instance.levelTable == null)
        {
            Debug.LogError("❌ LevelTable 数据表未加载！请检查 GlobalConfig 或 DataGenerator。");
            return false;
        }
        return true;
    }

    // =========================================================
    // UI 交互逻辑
    // =========================================================

    public void OpenSignInPanel()
    {
        if (signInManager != null)
        {
            signInManager.Show();
        }
        else
        {
            Debug.LogError("❌ MainMenu: signInManager 引用丢失！");
        }
    }

    public void StartMainUI(bool _isOpen)
    {
        // 🔥 1. 更新全局状态
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.isLevelSelectionOpen = _isOpen;
        }
        if (mainUI != null) mainUI.SetActive(_isOpen);
    }

    public void StartChooseLevel(bool _isOpen)
    {
        // 🔥 1. 更新全局状态
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.isLevelSelectionOpen = _isOpen;
        }

        if (chooseLevelUI != null) chooseLevelUI.SetActive(_isOpen);
    }

    // =========================================================
    // 🔥 修改点 2：在打开角色界面时判断是否播放漫画
    // =========================================================
    public void StartRoleUI(bool _isOpen)
    {
        // 如果是“打开”操作，且剧情播放器存在
        if (_isOpen && storyPlayer != null)
        {
            // 检查是否看过漫画 (0=没看过, 1=看过)
            bool hasWatched = PlayerPrefs.GetInt(PREF_STORY_WATCHED, 0) == 1;

            if (!hasWatched)
            {
                // === 第一次进入 ===
                // 1. 播放漫画，并传入“播完后要做的事”
                storyPlayer.PlayStory(() => {
                    Debug.Log("首次角色界面触发：漫画播放完毕");

                    // 2. 标记已看过
                    PlayerPrefs.SetInt(PREF_STORY_WATCHED, 1);
                    PlayerPrefs.Save();

                    // 3. 真正打开角色界面
                    if (roleUI != null) roleUI.SetActive(true);
                });

                // ⚠️ 这里直接返回，不执行下面的 SetActive，等待漫画播完的回调去执行
                return;
            }
        }

        // === 正常逻辑 (关闭界面，或已经是老玩家) ===
        if (roleUI != null) roleUI.SetActive(_isOpen);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("游戏已退出");
    }
}