using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CommonTipPanel : SimpleWindowUI
{
    [Header("=== 组件引用 ===")]
    public TextMeshProUGUI contentText;
    public Button confirmBtn;
    public TextMeshProUGUI btnText;

    private Action _onConfirmCallback;

    void Start()
    {
        if (confirmBtn)
        {
            confirmBtn.onClick.RemoveAllListeners();
            confirmBtn.onClick.AddListener(OnConfirmClicked);
        }
    }

    public void ShowTip(string content, Action onConfirm = null, string btnLabel = "确 定")
    {
        if (contentText)
        {
            contentText.text = content;

            // 🔥🔥🔥 核心：单行居中，多行靠左 🔥🔥🔥
            // 包含换行符 \n 或 长度超过20（根据卷轴宽度调整）则左对齐
            if (content.Contains("\n") || content.Length > 20)
                contentText.alignment = TextAlignmentOptions.Left;
            else
                contentText.alignment = TextAlignmentOptions.Center;
        }

        if (btnText) btnText.text = btnLabel;
        _onConfirmCallback = onConfirm;

        // 显示弹窗（SimpleWindowUI自带动画）
        base.Show();
        transform.SetAsLastSibling(); // 确保在最顶层
    }

    void OnConfirmClicked()
    {
        _onConfirmCallback?.Invoke();
        Hide();
    }
}