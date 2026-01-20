using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelMenuManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("=== 核心容器 ===")]
    public RectTransform contentParent;

    [Header("=== 功能按钮 ===")]
    public Button settingsButton;
    public Button startGameButton;
    public TextMeshProUGUI startBtnText;

    [Header("=== 翻页按钮 ===")]
    public Button topButton;      // 上箭头 (看上面的页)
    public Button bottomButton;   // 下箭头 (看下面的页)

    [Header("=== 生成设置 ===")]
    public GameObject mapChunkPrefab;
    public int levelsPerChunk = 5;
    public GameObject roleIcon;

    [Header("=== 滑动参数 ===")]
    public float fastSwipeThreshold = 1000f;
    public float fastSwipeMinMove = 50f;
    [Range(0.1f, 0.9f)]
    public float slowDragRatio = 0.5f;
    public float snapDuration = 0.25f;

    // --- 内部状态 ---
    // 🔥 强制固定高度 1920
    private float _pageHeight = 1920f;
    private int _totalPageCount = 0;
    private int _currentPageIndex = 0;
    private int _lastPlayedLevel = 1;
    private int _currentSelectedLevel = -1;
    private int _loadedMaxPage = -1;

    // --- 滑动计算 ---
    private float _startDragContentY;
    private float _startPointerY;
    private float _startTime;
    private bool _isAnimating = false;

    // 🔥 改为 IEnumerator 以等待 UI 布局完成
    IEnumerator Start()
    {
        // 1. 强制顶部对齐
        if (contentParent != null)
        {
            contentParent.pivot = new Vector2(0.5f, 1f);
            contentParent.anchorMin = new Vector2(0.5f, 1f);
            contentParent.anchorMax = new Vector2(0.5f, 1f);
        }

        // 读取存档
        if (SaveManager.Instance != null) _lastPlayedLevel = SaveManager.Instance.GetUnlockedLevel();
        if (GlobalConfig.Instance != null && GlobalConfig.Instance.currentLevelIndex > 0)
            _lastPlayedLevel = GlobalConfig.Instance.currentLevelIndex;

        _currentSelectedLevel = _lastPlayedLevel;

        if (roleIcon != null)
        {
            roleIcon.SetActive(false);
            if (roleIcon.GetComponent<Image>()) roleIcon.GetComponent<Image>().raycastTarget = false;
        }

        // 2. 生成所有页面
        CalculateTotalPages();
        for (int i = 0; i < _totalPageCount; i++) LoadPage(i);

        // 强制刷新一次布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

        // 🔥🔥🔥 关键修改：等待一帧，让 Unity 完成 Layout 计算，防止位置被弹回
        yield return null;

        // =========================================================
        // 核心：计算初始位置
        // =========================================================

        // 算出当前进度在第几页
        int targetPageIndex = (_lastPlayedLevel - 1) / levelsPerChunk;

        // 防止越界
        if (targetPageIndex < 0) targetPageIndex = 0;
        targetPageIndex = _totalPageCount - 1;

        _currentPageIndex = targetPageIndex;

        // 🔥 强制位置：页码 * 1920
        float initY = targetPageIndex * 1920f;

        if (contentParent != null)
        {
            contentParent.anchoredPosition = new Vector2(contentParent.anchoredPosition.x, initY);
        }
        // =========================================================

        UpdateButtonState();
        UpdateStartButtonState();
        InitRolePosition();

        // 3. 按钮绑定
        if (topButton)
        {
            topButton.onClick.RemoveAllListeners();
            // 上箭头 -> Index - 1 (回看上面)
            topButton.onClick.AddListener(() => GoToPage(_currentPageIndex - 1));
        }
        if (bottomButton)
        {
            bottomButton.onClick.RemoveAllListeners();
            // 下箭头 -> Index + 1 (看下面)
            bottomButton.onClick.AddListener(() => GoToPage(_currentPageIndex + 1));
        }

        if (settingsButton) settingsButton.onClick.AddListener(() => GlobalCanvas.Instance?.ToggleSettings());
        /*
        if (homeButton) homeButton.onClick.AddListener(() => SceneController.Instance?.LoadMainMenu());

        // 🔥🔥🔥 核心修改：Home 按钮逻辑
        if (homeButton)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => {
                // 1. 既然玩家点了 Home，说明他想回主标题界面，所以要把“保持在选关”的状态取消
                if (GlobalConfig.Instance != null)
                    GlobalConfig.Instance.isLevelSelectionOpen = false;

                // 2. 尝试直接调用 MainMenu 的方法切换（不用重载场景，更流畅）
                MainMenu mainMenu = FindObjectOfType<MainMenu>();
                if (mainMenu != null)
                {
                    mainMenu.StartMainUI(true);
                    mainMenu.StartRoleUI(false);
                    mainMenu.StartChooseLevel(false);
                }
                else
                {
                    // 保底：如果找不到 MainMenu 脚本，才重载场景
                    SceneController.Instance?.LoadMainMenu();
                }
            });
        }*/

        if (startGameButton)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(OnStartButtonClick);
        }
    }

    void InitRolePosition()
    {
        foreach (Transform child in contentParent)
        {
            MapChunk chunk = child.GetComponent<MapChunk>();
            if (chunk == null) continue;

            string[] nameParts = child.name.Split('_');
            if (nameParts.Length < 2) continue;

            if (int.TryParse(nameParts[1], out int pageIndex))
            {
                int startLevel = pageIndex * levelsPerChunk + 1;
                CheckAndPlaceRoleOnStart(chunk, startLevel);
            }
        }
    }

    void CalculateTotalPages()
    {
        int totalLevels = 8;
        if (GlobalConfig.Instance?.levelTable != null) totalLevels = GlobalConfig.Instance.levelTable.allLevels.Count;
        _totalPageCount = Mathf.CeilToInt((float)totalLevels / levelsPerChunk);
        if (_totalPageCount < 1) _totalPageCount = 1;
    }

    void LoadPage(int pageIndex)
    {
        string pageName = $"Page_{pageIndex}";
        if (contentParent.Find(pageName) == null)
        {
            GameObject chunk = Instantiate(mapChunkPrefab, contentParent);
            chunk.name = pageName;

            MapChunk script = chunk.GetComponent<MapChunk>();
            if (script != null)
            {
                int startLevel = pageIndex * levelsPerChunk + 1;
                int totalLevels = GlobalConfig.Instance ? GlobalConfig.Instance.levelTable.allLevels.Count : _totalPageCount * levelsPerChunk;
                script.SetupChunk(startLevel, totalLevels, HandleLevelClick);
            }
            if (pageIndex > _loadedMaxPage) _loadedMaxPage = pageIndex;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isAnimating) return;
        StopAllCoroutines();
        _isAnimating = false;
        _startDragContentY = contentParent.anchoredPosition.y;
        _startPointerY = eventData.position.y;
        _startTime = Time.time;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isAnimating) return;
        float pointerDelta = eventData.position.y - _startPointerY;

        // 往上滑(Delta > 0) -> 内容Y变大 -> 看下面的内容
        float targetY = _startDragContentY + pointerDelta;

        float minLimit = 0f;
        float maxLimit = (_totalPageCount - 1) * 1920f;
        targetY = Mathf.Clamp(targetY, minLimit, maxLimit);

        contentParent.anchoredPosition = new Vector2(contentParent.anchoredPosition.x, targetY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isAnimating) return;
        float dis = contentParent.anchoredPosition.y - _startDragContentY;
        if (Mathf.Abs(dis) < 1f) { GoToPage(_currentPageIndex); return; }

        int target = _currentPageIndex;
        if ((Mathf.Abs(dis / 1920f) > slowDragRatio) || (Mathf.Abs(dis) > fastSwipeMinMove && Mathf.Abs(dis / (Time.time - _startTime)) > fastSwipeThreshold))
        {
            if (dis > 0) target++;
            else target--;
        }
        GoToPage(target);
    }

    void GoToPage(int targetPage)
    {
        targetPage = Mathf.Clamp(targetPage, 0, _totalPageCount - 1);
        StartCoroutine(SmoothSnapToPage(targetPage));
    }

    IEnumerator SmoothSnapToPage(int targetPageIndex)
    {
        _isAnimating = true;
        _currentPageIndex = targetPageIndex;
        UpdateButtonState();

        float targetY = targetPageIndex * 1920f;
        float startY = contentParent.anchoredPosition.y;
        float timer = 0f;
        while (timer < snapDuration)
        {
            timer += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - (timer / snapDuration), 4f);
            contentParent.anchoredPosition = new Vector2(contentParent.anchoredPosition.x, Mathf.Lerp(startY, targetY, t));
            yield return null;
        }
        contentParent.anchoredPosition = new Vector2(contentParent.anchoredPosition.x, targetY);
        _isAnimating = false;
    }

    public void HandleLevelClick(int levelIndex, LevelButton btnScript)
    {
        _currentSelectedLevel = levelIndex;
        MoveRoleToButton(btnScript);
        UpdateStartButtonState();
    }

    void CheckAndPlaceRoleOnStart(MapChunk chunk, int startLevelOfChunk)
    {
        for (int i = 0; i < chunk.levelButtons.Count; i++)
        {
            if ((startLevelOfChunk + i) == _currentSelectedLevel)
            {
                MoveRoleToButton(chunk.levelButtons[i]);
                break;
            }
        }
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

    void UpdateButtonState()
    {
        if (topButton) topButton.gameObject.SetActive(_currentPageIndex > 0);
        if (bottomButton) bottomButton.gameObject.SetActive(_currentPageIndex < _totalPageCount - 1);
    }

    void UpdateStartButtonState()
    {
        if (startGameButton == null) return;
        startGameButton.interactable = true;
        if (startBtnText != null)
        {
            string levelName = "未知关卡";
            if (GlobalConfig.Instance?.levelTable?.allLevels != null && _currentSelectedLevel - 1 < GlobalConfig.Instance.levelTable.allLevels.Count && _currentSelectedLevel - 1 >= 0)
                levelName = GlobalConfig.Instance.levelTable.allLevels[_currentSelectedLevel - 1].displayTitle;
            startBtnText.text = $"开始挑战\n<size=40>{levelName}</size>";
        }
    }

    void OnStartButtonClick()
    {
        if (_currentSelectedLevel <= 0) return;
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.currentLevelIndex = _currentSelectedLevel;
            int configIndex = _currentSelectedLevel - 1;
            if (configIndex >= 0 && configIndex < GlobalConfig.Instance.levelTable.allLevels.Count)
                GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[configIndex];
        }
        if (SceneController.Instance != null) SceneController.Instance.LoadBattle();
        else SceneManager.LoadScene("BattleScene");
    }

    void CalculatePageHeight() { }
}