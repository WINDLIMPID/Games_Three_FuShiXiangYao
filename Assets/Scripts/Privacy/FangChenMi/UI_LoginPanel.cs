using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_LoginPanel : SimpleWindowUI
{
    [Header("=== 引用：注册界面 ===")]
    public UI_RegisterPanel registerPanel;

    [Header("=== 登录组件 ===")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginBtn;
    public Button goToRegisterBtn;

    [Header("=== 公共组件 ===")]
    public TextMeshProUGUI statusText;
    public Button userAgreementBtn;
    public Button privacyPolicyBtn;

    public string userAgreementUrl = "http://www.yourgame.com/agreement";
    public string privacyPolicyUrl = "http://www.yourgame.com/privacy";

    void Start()
    {
        if (loginBtn) loginBtn.onClick.AddListener(OnLoginClicked);
        if (goToRegisterBtn) goToRegisterBtn.onClick.AddListener(OnGoToRegisterClicked);
        if (userAgreementBtn) userAgreementBtn.onClick.AddListener(() => Application.OpenURL(userAgreementUrl));
        if (privacyPolicyBtn) privacyPolicyBtn.onClick.AddListener(() => Application.OpenURL(privacyPolicyUrl));
    }

    public override void Show()
    {
        base.Show();

        if (AccountManager.Instance != null && usernameInput != null)
        {
            // 1. 回填账号
            string lastUser = AccountManager.Instance.GetLastUsedUsername();
            usernameInput.text = lastUser;

            // 2. 🔥 回填密码 (只有登录成功过的号才有)
            if (!string.IsNullOrEmpty(lastUser) && passwordInput != null)
            {
                string savedPass = AccountManager.Instance.GetLocalPassword(lastUser);
                passwordInput.text = savedPass;
            }

            // 体验优化：如果账号密码都有，直接准备点登录；只有账号没密码，跳到密码框
            if (!string.IsNullOrEmpty(lastUser) && string.IsNullOrEmpty(passwordInput.text))
            {
                passwordInput.ActivateInputField();
            }
        }

        if (statusText) statusText.text = "";
    }

    void OnGoToRegisterClicked()
    {
        Hide();
        if (registerPanel != null) registerPanel.Show();
    }

    void OnLoginClicked()
    {
        string u = usernameInput.text.Trim();
        string p = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(u)) { statusText.text = "请输入账号"; return; }
        if (string.IsNullOrEmpty(p)) { statusText.text = "请输入密码"; return; }

        loginBtn.interactable = false;
        statusText.text = "正在登录...";

        AccountManager.Instance.Login(u, p, (success, msg) => {
            loginBtn.interactable = true;
            if (success)
            {
                statusText.text = "登录成功！";

                Hide();

                // 🔥🔥🔥 加上这一行！初始化防沉迷数据 🔥🔥🔥
                if (AntiAddictionManager.Instance != null)
                {
                    AntiAddictionManager.Instance.InitData(usernameInput.text);
                }

            }
            else
            {
                // 🔥 修改：只有短消息（如密码错误）才显示在面板上
                // 如果消息很长（包含换行），说明是防沉迷提示，直接清空面板文字
                if (msg.Length > 15 || msg.Contains("\n"))
                {
                    statusText.text = "";
                }
                else if (msg.Contains("不存在") || msg.Contains("密码错误"))
                {
                    statusText.text = "账号或密码错误";
                }
                else
                {
                    statusText.text = msg;
                }
            }
        });
    }

    protected override void OnHideComplete()
    {
        if (registerPanel == null || !registerPanel.gameObject.activeSelf)
        {
            GameObject.FindObjectOfType<MainMenu>()?.CheckVerificationFlow();
        }
    }
}