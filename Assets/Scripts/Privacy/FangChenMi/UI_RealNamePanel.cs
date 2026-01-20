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

        // 基础格式校验
        if (n.Length < 2 || (id.Length != 15 && id.Length != 18))
        {
            statusText.text = "<color=red>请输入有效的姓名和身份证号</color>";
            return;
        }

        // 🔥🔥🔥 核心：获取当前是谁在登录 🔥🔥🔥
        string currentUsername = "";
        if (AccountManager.Instance != null)
        {
            currentUsername = AccountManager.Instance.GetLastUsedUsername();
        }

        // 异常处理：如果没登录就想实名（通常不会发生），给个临时ID
        if (string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogError("❌ 警告：未获取到当前账号名！");
            statusText.text = "<color=red>账号状态异常，请重新登录</color>";
            return;
        }

        statusText.text = "正在认证...";
        submitBtn.interactable = false;

        Debug.Log($"📝 正在为账号 [{currentUsername}] 提交实名认证...");

        // 发送请求时，传入 currentUsername
        AntiAddictionManager.Instance.RequestVerify(n, id, currentUsername, (success) => {
            submitBtn.interactable = true;
            if (success)
            {
                statusText.text = "<color=green>认证成功！</color>";

                // 成功后1秒关闭界面
                Invoke("ClosePanel", 1.0f);
            }
            else
            {
                statusText.text = "<color=red>认证失败，信息不匹配</color>";
            }
        });
    }

    void ClosePanel() => gameObject.SetActive(false);
}