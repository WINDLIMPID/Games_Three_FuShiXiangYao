using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PrivacyPanel : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject panelRoot;   // 整个弹窗界面
    public Transform windowBox;    // 中间的那个弹窗框 (用于做动画)
    public Button agreeBtn;        // 同意按钮 (绿色)
    public Button exitBtn;         // 不同意/退出按钮 (灰色)

    [Header("协议链接 (必填)")]
    // 这里填你公司官网或者在线文档的地址，如果没有，先填个百度的做测试
    public string userAgreementUrl = "http://www.yourgame.com/agreement";
    public string privacyPolicyUrl = "http://www.yourgame.com/privacy";

    private const string PREFS_KEY = "HasAgreedPrivacy_V1"; // 版本号V1，以后改了协议可以换成V2让玩家重签

    void Start()
    {
        // 1. 检查是否已经同意过
        if (PlayerPrefs.GetInt(PREFS_KEY, 0) == 1)
        {
            // 已经同意过，直接销毁自己，不挡路
            Destroy(gameObject);
            return;
        }

        // 2. 没同意过，显示弹窗，并暂停游戏逻辑（防止主界面背景乱动）
        ShowPanel();
    }

    void ShowPanel()
    {
        panelRoot.SetActive(true);

        // 简单的果冻弹出动画
        windowBox.localScale = Vector3.zero;
        windowBox.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        // 绑定按钮事件
        agreeBtn.onClick.AddListener(OnAgree);
        exitBtn.onClick.AddListener(OnExit);
    }

    public void OpenUserAgreement()
    {
        Application.OpenURL(userAgreementUrl);
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(privacyPolicyUrl);
    }

    void OnAgree()
    {
        // 1. 记录状态：玩家已同意
        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();

        // 2. 播放关闭动画并进入游戏
        windowBox.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    void OnExit()
    {
        // 硬性规定：不同意必须退出游戏
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}