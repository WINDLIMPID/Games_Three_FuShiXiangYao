using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelButton : MonoBehaviour
{
    [Header("UI 组件引用")]
    public TextMeshProUGUI titleText;
    public Button clickButton;
    public GameObject lockIcon;
    public Image previewImage; // 如果 prefab 里有预览图位置，可以拖进来

    // 内部数据
    private int _levelIndex;

    // 委托：把自己的 index 和 脚本本身 传回去，方便小人定位
    private Action<int, LevelButton> _onClickedCallback;

    /// <summary>
    /// 初始化方法
    /// </summary>
    public void Setup(int levelIndex, LevelConfigEntry data, bool isLocked, Action<int, LevelButton> onClickedCallback)
    {
        _levelIndex = levelIndex;
        _onClickedCallback = onClickedCallback;

        // 1. 设置文字
        if (titleText != null)
        {
            // 优先用配置表的标题，如果没有就显示“第N关”
            string t = data != null && !string.IsNullOrEmpty(data.displayTitle) ? data.displayTitle : $"第 {levelIndex} 关";
            titleText.text = t;
        }

        // 2. 设置图片 (如果有)
        if (previewImage != null && data != null)
        {
            previewImage.sprite = data.previewImage;
        }

        // 3. 锁状态
        if (lockIcon != null) lockIcon.SetActive(isLocked);

        // 4. 按钮点击事件
        if (clickButton != null)
        {
            clickButton.interactable = !isLocked; // 锁住不能点
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() => {
                // 触发回调，告诉 Manager "我被点了"
                if (_onClickedCallback != null)
                {
                    _onClickedCallback.Invoke(_levelIndex, this);
                }
            });
        }
    }
}