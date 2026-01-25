using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelButton : MonoBehaviour
{
    [Header("UI 组件引用 (代码自动获取)")]
    public Button clickButton;
    public GameObject lockIcon;

    public int _levelIndex;
    private Action<int, LevelButton> _onClickedCallback;

    private void Awake()
    {
        // 🔥 自动寻找子物体中的按钮组件，省去手动拖拽
        if (clickButton == null)
            clickButton = GetComponentInChildren<Button>();
    }

    public void Setup(int levelIndex, LevelConfigEntry data, bool isLocked, Action<int, LevelButton> onClickedCallback)
    {
        _levelIndex = levelIndex;
        _onClickedCallback = onClickedCallback;

        // 保底检查
        if (clickButton == null) clickButton = GetComponentInChildren<Button>();

        if (lockIcon != null) lockIcon.SetActive(isLocked);

        if (clickButton != null)
        {
            clickButton.interactable = !isLocked;
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() => {
                _onClickedCallback?.Invoke(_levelIndex, this);
            });
        }
    }
}