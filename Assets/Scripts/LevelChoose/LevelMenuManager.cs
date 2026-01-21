using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引用 TMP 命名空间
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class LevelMenuManager : MonoBehaviour
{
    [Header("=== 核心组件 ===")]
    public ScrollRect scrollView;
    public RectTransform contentRect;
    public GameObject roleIcon;

    [Header("=== 关卡按钮配置 ===")]
    // 建议手动按顺序拖拽 LevelButton 到这里
    public List<LevelButton> allLevelButtons = new List<LevelButton>();

    [Header("=== 功能按钮 ===")]
    public Button settingsButton;
    public Button startGameButton;      // "开始挑战" 按钮
    public TextMeshProUGUI startBtnText; // 按钮上的文字 (可选)

    // --- 内部状态 ---
    private int _unlockedLevelCount = 1;
    private int _currentSelectedLevel = -1; // 当前选中的关卡

    IEnumerator Start()
    {
        // 1. 读取进度
        LoadProgress();

        // 默认选中最新解锁的关卡
        _currentSelectedLevel = _unlockedLevelCount;

        // 2. 初始化所有按钮
        RefreshLevelButtons();

        // 3. 放置小人 & 聚焦
        PlaceRoleOnLevel(_unlockedLevelCount);

        // 4. 刷新开始按钮状态 (显示最新关卡名字)
        UpdateStartButtonState();

        yield return null;

        // 5. 聚焦到最新关卡
        FocusOnLevel(_unlockedLevelCount);

        // 6. 绑定按钮事件
        BindButtons();
    }

    void BindButtons()
    {
        if (settingsButton)
            settingsButton.onClick.AddListener(() => GlobalCanvas.Instance?.ToggleSettings());

        // 🔥 绑定开始按钮事件
        if (startGameButton)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartButtonClick);
        }
    }

    // =========================================================
    // 🔥 核心交互逻辑
    // =========================================================

    // 点击关卡平台时触发 (只选中，不进游戏)
    void OnLevelButtonClicked(int levelIndex, LevelButton btn)
    {
        _currentSelectedLevel = levelIndex;

        // 1. 小人跳过去
        MoveRoleToButton(btn);

        // 2. 刷新开始按钮的文字/状态
        UpdateStartButtonState();

        // ❌ 删除了 EnterLevel(levelIndex)，现在点击平台不会直接进游戏了
    }

    // 点击“开始挑战”按钮时触发
    void OnStartButtonClick()
    {
        // 只有选中的关卡有效时才进入
        if (_currentSelectedLevel > 0)
        {
            EnterLevel(_currentSelectedLevel);
        }
    }

    // 刷新开始按钮的显示
    void UpdateStartButtonState()
    {
        if (startGameButton == null) return;

        // 确保有选中关卡
        bool hasSelection = _currentSelectedLevel > 0;
        startGameButton.interactable = hasSelection;

        // 如果有文字组件，更新显示 (例如: "开始挑战 第5关")
        if (startBtnText != null && hasSelection)
        {
            string levelName = $"第 {_currentSelectedLevel} 关";

            // 尝试获取配置里的名字
            if (GlobalConfig.Instance?.levelTable?.allLevels != null)
            {
                int index = _currentSelectedLevel - 1;
                if (index >= 0 && index < GlobalConfig.Instance.levelTable.allLevels.Count)
                {
                    levelName = GlobalConfig.Instance.levelTable.allLevels[index].displayTitle;
                }
            }

            startBtnText.text = "开始挑战\n<size=40>" + levelName + "</size>";
        }
    }

    // =========================================================
    // 基础功能
    // =========================================================

    void LoadProgress()
    {
        _unlockedLevelCount = 1;
        if (SaveManager.Instance != null)
        {
            _unlockedLevelCount = SaveManager.Instance.GetUnlockedLevel();
        }
        else if (GlobalConfig.Instance != null && GlobalConfig.Instance.currentLevelIndex > 0)
        {
            _unlockedLevelCount = GlobalConfig.Instance.currentLevelIndex;
        }

        if (allLevelButtons.Count > 0)
            _unlockedLevelCount = Mathf.Clamp(_unlockedLevelCount, 1, allLevelButtons.Count);
    }

    void RefreshLevelButtons()
    {
        if (allLevelButtons == null || allLevelButtons.Count == 0)
        {
            allLevelButtons = contentRect.GetComponentsInChildren<LevelButton>()
                .OrderBy(b => b.gameObject.name.Length)
                .ThenBy(b => b.gameObject.name)
                .ToList();
        }

        for (int i = 0; i < allLevelButtons.Count; i++)
        {
            int levelIndex = i + 1;
            LevelButton btn = allLevelButtons[i];

            LevelConfigEntry data = null;
            if (GlobalConfig.Instance?.levelTable?.allLevels != null && i < GlobalConfig.Instance.levelTable.allLevels.Count)
            {
                data = GlobalConfig.Instance.levelTable.allLevels[i];
            }

            bool isLocked = levelIndex > _unlockedLevelCount;
            btn.Setup(levelIndex, data, isLocked, OnLevelButtonClicked);
        }
    }

    void PlaceRoleOnLevel(int levelIndex)
    {
        if (levelIndex <= 0 || levelIndex > allLevelButtons.Count) return;
        MoveRoleToButton(allLevelButtons[levelIndex - 1]);
    }

    void MoveRoleToButton(LevelButton btn)
    {
        if (roleIcon == null) return;
        roleIcon.SetActive(true);
        roleIcon.transform.SetParent(btn.transform);
        RectTransform rt = roleIcon.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 50);
        rt.localScale = Vector3.one;
    }

    void FocusOnLevel(int levelIndex)
    {
        if (scrollView == null || contentRect == null) return;
        if (levelIndex <= 0 || levelIndex > allLevelButtons.Count) return;

        LevelButton targetBtn = allLevelButtons[levelIndex - 1];
        RectTransform targetRect = targetBtn.GetComponent<RectTransform>();

        float viewportHeight = scrollView.viewport.rect.height;
        float contentHeight = contentRect.rect.height;
        float targetY = targetRect.anchoredPosition.y;
        float finalContentY = (viewportHeight / 2f) - targetY;

        float maxY = 0f;
        float minY = -(contentHeight - viewportHeight);

        if (contentHeight < viewportHeight) finalContentY = 0;
        else finalContentY = Mathf.Clamp(finalContentY, minY, maxY);

        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, finalContentY);
    }

    public void EnterLevel(int levelIndex)
    {
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.currentLevelIndex = levelIndex;
            if (GlobalConfig.Instance.levelTable != null && (levelIndex - 1) < GlobalConfig.Instance.levelTable.allLevels.Count)
            {
                GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[levelIndex - 1];
            }
        }

        if (SceneController.Instance != null) SceneController.Instance.LoadBattle();
        else SceneManager.LoadScene("BattleScene");
    }
}