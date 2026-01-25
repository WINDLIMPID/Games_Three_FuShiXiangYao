using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_RealNamePanel : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField idCardInput;
    public Button submitBtn;
    public TextMeshProUGUI statusText;

    void Start()
    {
        if (submitBtn) submitBtn.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        string n = nameInput.text.Trim();
        string id = idCardInput.text.Trim();

        if (n.Length < 2 || (id.Length != 15 && id.Length != 18))
        {
            statusText.color = Color.red;
            statusText.text = "请输入有效的姓名和身份证号";
            return;
        }

        string currentUsername = "";
        if (AccountManager.Instance != null)
        {
            currentUsername = AccountManager.Instance.GetLastUsedUsername();
        }

        if (string.IsNullOrEmpty(currentUsername))
        {
            statusText.text = "账号异常，请重新登录";
            return;
        }

        statusText.color = Color.white;
        statusText.text = "正在认证...";
        submitBtn.interactable = false;

        AntiAddictionManager.Instance.RequestVerify(n, id, currentUsername, (success, msg) => {
            submitBtn.interactable = true;

            if (success)
            {
                // 1. 获取限制文案
                int age = AntiAddictionManager.Instance.currentUserAge;
                string limitMsg = AntiAddictionManager.Instance.CheckLoginLimit(age);

                // 2. 如果有限制文案 -> 弹出大卷轴
                if (!string.IsNullOrEmpty(limitMsg))
                {
                    statusText.text = ""; // 清空小红字，避免重复

                    if (GlobalCanvas.Instance != null)
                    {
                        // 弹窗提示，且只有一个“确定”按钮
                        GlobalCanvas.Instance.ShowTip(limitMsg, null, "我知道了");
                    }

                    Debug.LogWarning("[UI] 实名成功但被限制: " + limitMsg);
                    // 不关闭界面，阻止进入游戏
                }
                else
                {
                    // 3. 如果无限制 -> 绿字提示 -> 进游戏
                    statusText.color = Color.green;
                    statusText.text = "认证成功，祝您游戏愉快！";

                    Invoke("ClosePanel", 1.5f);

                    MainMenu mainMenu = FindObjectOfType<MainMenu>();
                    if (mainMenu != null)
                    {
                        mainMenu.CheckVerificationFlow();
                    }
                }
            }
            else
            {
                // 4. 认证失败（普通错误）-> 只显示红字，不弹窗
                statusText.color = Color.red;
                statusText.text = msg;
            }
        });
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}