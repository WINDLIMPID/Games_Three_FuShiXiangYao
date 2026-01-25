using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;
using System;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("=== 第一阶段：黑屏开始 ===")]
    public GameObject blackPanel;
    public Button startBtn;

    [Header("=== 第二阶段：滑动引导 ===")]
    public GameObject tutorialMask;
    public RectTransform focusArea;
    public RectTransform handIcon;
    public TextMeshProUGUI tipText;

    [Header("=== 摇杆引用 ===")]
    public Joystick playerJoystick;
    public RectTransform joystickTouchZone;
    public Vector2 tutorialZoneSize = new Vector2(300, 300);
    public Vector2 tutorialZonePos = new Vector2(0, -400);

    private Vector2 _originalSize;
    private Vector2 _originalPos;
    private Action _onCompleteCallback;

    void Awake()
    {
        Instance = this;
        if (blackPanel) blackPanel.SetActive(false);
        if (tutorialMask) tutorialMask.SetActive(false);
        if (handIcon) handIcon.gameObject.SetActive(false);
    }

    // 🔥🔥🔥 Start 留空！等待 LevelIntroUI 调用 🔥🔥🔥
    void Start() { }

    // 🔥 这个方法由 LevelIntroUI 动画结束后调用
    public void CheckAndStartTutorial(Action onComplete)
    {
        _onCompleteCallback = onComplete;

        bool isFinished = false;
        if (SaveManager.Instance != null) isFinished = SaveManager.Instance.IsTutorialComplete();

        if (isFinished)
        {
            // === 老手 ===
            Debug.Log("TutorialManager: 老手，跳过引导 -> 通知刷怪");
            // 直接执行回调（去刷怪）
            _onCompleteCallback?.Invoke();
            Destroy(gameObject);
        }
        else
        {
            // === 新手 ===
            Debug.Log("TutorialManager: 新手，开始黑屏流程");
            StartBlackScreenPhase();
        }
    }

    void StartBlackScreenPhase()
    {
        if (blackPanel) blackPanel.SetActive(true);
        if (startBtn)
        {
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(OnStartBtnClicked);
        }
    }

    void OnStartBtnClicked()
    {
        if (blackPanel) blackPanel.SetActive(false);
        StartCoroutine(SlideTutorialFlow());
    }

    IEnumerator SlideTutorialFlow()
    {
        if (joystickTouchZone != null)
        {
            _originalSize = joystickTouchZone.sizeDelta;
            _originalPos = joystickTouchZone.anchoredPosition;
            joystickTouchZone.sizeDelta = tutorialZoneSize;
            joystickTouchZone.anchoredPosition = tutorialZonePos;
        }

        if (tutorialMask) tutorialMask.SetActive(true);
        if (tipText) tipText.text = "滑动屏幕 移动角色";

        Sequence handSeq = DOTween.Sequence();
        if (handIcon)
        {
            handIcon.gameObject.SetActive(true);
            handIcon.anchoredPosition = tutorialZonePos;
            if (focusArea) focusArea.anchoredPosition = tutorialZonePos;
            handSeq.SetLoops(-1, LoopType.Yoyo);
            handIcon.DOLocalMoveY(tutorialZonePos.y + 100, 1f).SetLoops(-1, LoopType.Yoyo);
        }

        float moveTimer = 0f;
        while (moveTimer < 0.3f)
        {
            if (playerJoystick != null && new Vector2(playerJoystick.Horizontal, playerJoystick.Vertical).magnitude > 0.1f)
            {
                moveTimer += Time.deltaTime;
                if (handIcon) handIcon.gameObject.SetActive(false);
            }
            else
            {
                moveTimer = 0f;
                if (handIcon) handIcon.gameObject.SetActive(true);
            }
            yield return null;
        }

        handSeq.Kill();
        if (joystickTouchZone != null)
        {
            joystickTouchZone.sizeDelta = _originalSize;
            joystickTouchZone.anchoredPosition = _originalPos;
        }
        if (tutorialMask) tutorialMask.SetActive(false);

        if (SaveManager.Instance != null) SaveManager.Instance.CompleteTutorial();

        // 🔥 引导结束 -> 通知刷怪
        Debug.Log("TutorialManager: 引导完成 -> 通知刷怪");
        _onCompleteCallback?.Invoke();

        Destroy(gameObject);
    }
}