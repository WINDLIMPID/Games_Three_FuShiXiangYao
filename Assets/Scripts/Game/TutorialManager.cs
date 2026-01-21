using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI 容器")]
    public GameObject tutorialMask;

    [Header("UI 子组件")]
    public RectTransform focusArea;
    public RectTransform handIcon;
    public TextMeshProUGUI tipText;

    [Header("摇杆控制")]
    public Joystick playerJoystick;
    public RectTransform joystickTouchZone;

    [Header("配置")]
    public Vector2 tutorialZoneSize = new Vector2(300, 300);
    public Vector2 tutorialZonePos = new Vector2(0, -400);

    private Vector2 _originalSize;
    private Vector2 _originalPos;

    public SimpleWindowUI desImage;     //介绍UI

    void Awake()
    {
        Instance = this;
        // 强制初始化关闭
        if (tutorialMask != null) tutorialMask.SetActive(false);
    }

    void Start()
    {
        // 检查存档
        if (PlayerPrefs.GetInt("IsTutorialFinished", 0) == 1)
        {
            if (EnemySpawner.Instance) EnemySpawner.Instance.StartSpawning();
            Destroy(gameObject); // 老玩家直接销毁脚本
            return;
        }
        desImage.Show();
        
    }

    public void StartSimpleTutorialFlow()
    {
        desImage.Hide();

        // 新手开始引导

        StartCoroutine(SimpleTutorialFlow());
    }

    IEnumerator SimpleTutorialFlow()
    {

        

        // --- 1. 准备 ---
        _originalSize = joystickTouchZone.sizeDelta;
        _originalPos = joystickTouchZone.anchoredPosition;

        joystickTouchZone.sizeDelta = tutorialZoneSize;
        joystickTouchZone.anchoredPosition = tutorialZonePos;

        tutorialMask.SetActive(true);
        if (tipText) tipText.text = "滑动屏幕 控制角色";

        // 动画
        
        Sequence handSeq = DOTween.Sequence();
        if (handIcon)
        {
            handIcon.gameObject.SetActive(true);
            handIcon.anchoredPosition = tutorialZonePos;
            focusArea.anchoredPosition = tutorialZonePos;

            //handSeq.Append(handIcon.DOScale(0.8f, 0.5f));
            //handSeq.Append(handIcon.DOScale(1.2f, 0.5f));
            handSeq.SetLoops(-1, LoopType.Yoyo);
        }

        // --- 2. 检测 ---
        float moveTimer = 0f;
        while (moveTimer < 0.5f)
        {
            if (new Vector2(playerJoystick.Horizontal, playerJoystick.Vertical).magnitude > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (handIcon && handIcon.gameObject.activeSelf) handIcon.gameObject.SetActive(false);
            }
            else
            {
                moveTimer = 0f;
                if (handIcon && !handIcon.gameObject.activeSelf) handIcon.gameObject.SetActive(true);
            }
            yield return null;
        }

        // --- 3. 完成 ---
        handSeq.Kill();
        joystickTouchZone.sizeDelta = _originalSize;
        joystickTouchZone.anchoredPosition = _originalPos;
        tutorialMask.SetActive(false);

        // 存档
        PlayerPrefs.SetInt("IsTutorialFinished", 1);
        PlayerPrefs.Save();

        if (EnemySpawner.Instance) EnemySpawner.Instance.StartSpawning();

        Destroy(gameObject);
    }

    // =====================================================
    // 🔥【新增】右键菜单重置 & F9 快捷键
    // =====================================================

    [ContextMenu("🔴 重置新手引导")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("IsTutorialFinished");
        PlayerPrefs.Save();
        Debug.Log("【TutorialManager】新手引导已重置！请重新运行游戏。");
    }

#if UNITY_EDITOR
    void Update()
    {
        // 开发时按 F9 键：重置数据 + 重启场景 = 立即重新测试引导
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ResetTutorial();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
#endif
}