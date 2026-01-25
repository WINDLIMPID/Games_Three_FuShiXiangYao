using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PrivacyPanel : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject panelRoot;
    public Transform windowBox;
    public Button agreeBtn;
    public Button exitBtn;

    [Header("协议链接")]
    public string userAgreementUrl = "http://www.yourgame.com/agreement";
    public string privacyPolicyUrl = "http://www.yourgame.com/privacy";

    // 这个 Key 决定了它是跟设备绑定的
    private const string PREFS_KEY = "HasAgreedPrivacy_V1";

    void Start()
    {
        // 1. 检查是否已经同意过 (设备级检查)
        if (PlayerPrefs.GetInt(PREFS_KEY, 0) == 1)
        {
            Destroy(gameObject); // 同意过就销毁
            return;
        }

        // 2. 没同意过，显示
        ShowPanel();
    }

    void ShowPanel()
    {
        panelRoot.SetActive(true);
        windowBox.localScale = Vector3.zero;
        windowBox.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        agreeBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();

        agreeBtn.onClick.AddListener(OnAgree);
        exitBtn.onClick.AddListener(OnExit);
    }

    public void OpenUserAgreement() => Application.OpenURL(userAgreementUrl);
    public void OpenPrivacyPolicy() => Application.OpenURL(privacyPolicyUrl);

    void OnAgree()
    {
        // 记录同意状态
        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();

        windowBox.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 🔥🔥🔥 调试用：右键点击组件 -> Reset Privacy 可以重置状态 🔥🔥🔥
    [ContextMenu("Reset Privacy Status")]
    public void ClearPrivacyData()
    {
        PlayerPrefs.DeleteKey(PREFS_KEY);
        Debug.Log("隐私协议状态已重置！下次运行会重新弹窗。");
    }
}