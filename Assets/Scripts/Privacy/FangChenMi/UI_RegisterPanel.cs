using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_RegisterPanel : SimpleWindowUI
{
    [Header("=== 引用：登录界面 ===")]
    public UI_LoginPanel loginPanel; // 需拖入 LoginPanel

    [Header("=== 注册输入 ===")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPassInput;

    [Header("=== 按钮 ===")]
    public Button submitBtn;
    public Button backBtn;

    [Header("=== 状态显示 ===")]
    public TextMeshProUGUI statusText;

    void Start()
    {
        if (submitBtn) submitBtn.onClick.AddListener(OnSubmitClicked);
        if (backBtn) backBtn.onClick.AddListener(OnBackClicked);
    }

    public override void Show()
    {
        base.Show();
        // 清空输入框
        if (usernameInput) usernameInput.text = "";
        if (passwordInput) passwordInput.text = "";
        if (confirmPassInput) confirmPassInput.text = "";
        if (statusText) statusText.text = "注册新账号";
    }

    void OnBackClicked()
    {
        Hide(); // 关闭注册
        if (loginPanel != null) loginPanel.Show(); // 返回登录
    }

    void OnSubmitClicked()
    {
        string u = usernameInput.text.Trim();
        string p = passwordInput.text.Trim();
        string cp = confirmPassInput.text.Trim();

        // 🔥 本地校验 + 简短提示
        if (u.Length < 4 || u.Length > 20) { statusText.text = "账号限4-20位"; return; }
        if (p.Length < 8 || p.Length > 16) { statusText.text = "密码限8-16位"; return; }
        if (p != cp) { statusText.text = "两次密码不一致"; return; }

        submitBtn.interactable = false;
        statusText.text = "正在提交...";

        AccountManager.Instance.Register(u, p, (success, msg) => {
            submitBtn.interactable = true;
            if (success)
            {
                // 注册成功，切回登录界面
                if (loginPanel != null)
                {
                    loginPanel.usernameInput.text = u; // 填账号
                    loginPanel.passwordInput.text = ""; // 🔥 密码留空，强迫玩家记忆
                    loginPanel.Show();
                    this.Hide();

                    if (loginPanel.statusText)
                        loginPanel.statusText.text = "注册成功，请登录";
                }
            }
            else
            {
                if (msg.Contains("已存在")) statusText.text = "账号已存在";
                else statusText.text = msg.ToString();
            }
        });
    }
}