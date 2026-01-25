using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelMenuManager : MonoBehaviour
{
    [Header("=== 核心组件 ===")]
    public ScrollRect scrollView;
    public RectTransform contentRect;
    public GameObject roleIcon;
    public GameObject storyPanel; // 漫画面板

    [Header("=== 关卡按钮配置 ===")]
    public List<LevelButton> allLevelButtons = new List<LevelButton>();

    [Header("=== 功能按钮 ===")]
    public Button settingsButton;
    public Button startGameButton;
    public TextMeshProUGUI startBtnText;

    private int _unlockedLevelCount = 1;
    public int _currentSelectedLevel = -1;

    IEnumerator Start()
    {
        // 1. 获取存档进度
        if (SaveManager.Instance != null)
            _unlockedLevelCount = SaveManager.Instance.GetUnlockedLevel();
        else
            _unlockedLevelCount = 1;

        _currentSelectedLevel = _unlockedLevelCount;

        // 2. 刷新关卡按钮
        RefreshLevelButtons();

        // 3. 🔥 核心修复：根据 AccountTier 账号等级判断漫画显示
        CheckTierAndHideStory();

        // 4. 角色图标定位
        if (allLevelButtons.Count > 0)
        {
            // 如果是顶级账号（解锁100关），但地图只有8关
            // 我们需要把当前的逻辑选择限制在地图最大关卡内
            _currentSelectedLevel = Mathf.Min(_unlockedLevelCount, allLevelButtons.Count);

            PlaceRoleOnLevel(_unlockedLevelCount);
            UpdateStartButtonState();

            // 5. ScrollRect 百分比定位
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            float percent = (float)(_unlockedLevelCount - 1) / (allLevelButtons.Count - 1);
            scrollView.verticalNormalizedPosition = Mathf.Clamp01(percent);
        }

        BindButtons();
    }

    // 🔥 新增逻辑：对接 ComplianceDataManager 的账号分级
    void CheckTierAndHideStory()
    {
        if (storyPanel == null) return;

        // 获取当前登录的测试账号信息
        if (AccountManager.Instance != null && ComplianceDataManager.Instance != null)
        {
            // 通过当前用户名查找账号数据
            string user = AccountManager.Instance.currentUsername;
            string pwd = AccountManager.Instance.GetLocalPassword(user);
            TestAccountData accData = ComplianceDataManager.Instance.GetTestAccount(user, pwd);

            if (accData != null)
            {
                // 根据 AccountTier 进行判断
                // 只有 Blank (空白) 和 None (普通玩家) 需要看漫画
                // Senior (高级) 和 Intermediate (中级) 账号应跳过漫画
                if (accData.tier == AccountTier.Senior || accData.tier == AccountTier.Intermediate)
                {
                    storyPanel.SetActive(false);
                    Debug.Log($"🛠 [测试] 账号等级为 {accData.tier}，自动跳过漫画面板。");
                }
            }
        }
    }

    void BindButtons()
    {
        if (settingsButton)
            settingsButton.onClick.AddListener(() => GlobalCanvas.Instance?.ToggleSettings());

        if (startGameButton)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartButtonClick);
        }
    }

    void OnLevelButtonClicked(int levelIndex, LevelButton btn)
    {
        if (levelIndex > _unlockedLevelCount) return;
        _currentSelectedLevel = levelIndex;
        MoveRoleToButton(btn);
        UpdateStartButtonState();
    }

    void OnStartButtonClick()
    {
        if (_currentSelectedLevel != -1) EnterLevel(_currentSelectedLevel);
    }

    void UpdateStartButtonState()
    {
        if (startBtnText != null)
        {
            string levelName = $"第 {_currentSelectedLevel} 关";
            if (GlobalConfig.Instance?.levelTable?.allLevels != null)
            {
                int idx = _currentSelectedLevel - 1;
                if (idx >= 0 && idx < GlobalConfig.Instance.levelTable.allLevels.Count)
                    levelName = GlobalConfig.Instance.levelTable.allLevels[idx].displayTitle;
            }
            startBtnText.text = "开始挑战\n<size=40>" + levelName + "</size>";
        }
    }

    void RefreshLevelButtons()
    {
        if (allLevelButtons == null || allLevelButtons.Count == 0)
        {
            allLevelButtons = contentRect.GetComponentsInChildren<LevelButton>().ToList();
            allLevelButtons.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));
        }

        for (int i = 0; i < allLevelButtons.Count; i++)
        {
            allLevelButtons[i].Setup(i + 1, null, (i + 1) > _unlockedLevelCount, OnLevelButtonClicked);
        }
    }

    void PlaceRoleOnLevel(int levelIndex)
    {
        if (allLevelButtons.Count == 0) return;
        MoveRoleToButton(allLevelButtons[Mathf.Clamp(levelIndex - 1, 0, allLevelButtons.Count - 1)]);
    }

    void MoveRoleToButton(LevelButton btn)
    {
        if (roleIcon == null || btn == null) return;
        roleIcon.SetActive(true);
        roleIcon.transform.SetParent(btn.transform);
        RectTransform rt = roleIcon.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 50);
        rt.localScale = Vector3.one;
        rt.SetAsLastSibling();
    }

    public void EnterLevel(int levelIndex)
    {
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.currentLevelIndex = levelIndex;
            if (GlobalConfig.Instance.levelTable != null && (levelIndex - 1) < GlobalConfig.Instance.levelTable.allLevels.Count)
                GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[levelIndex - 1];
        }

        if (SceneController.Instance != null) SceneController.Instance.LoadBattle();
        else SceneManager.LoadScene("BattleScene");
    }
}