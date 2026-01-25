using UnityEngine;

public class GlobalCanvas : MonoBehaviour
{
    public static GlobalCanvas Instance;

    [Header("挂载界面")]
    public GameObject settingsPanel;

    [Header("🔥 通用弹窗引用")]
    public CommonTipPanel commonTipPanel; // 记得在Unity编辑器里把做好的卷轴弹窗拖到这里！

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        // 确保弹窗初始隐藏
        if (commonTipPanel != null) commonTipPanel.gameObject.SetActive(false);
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            bool isOpening = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isOpening);
            Time.timeScale = isOpening ? 0f : 1f;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    // 🔥🔥🔥 核心：提供全局弹窗调用接口 🔥🔥🔥
    public void ShowTip(string content, System.Action onConfirm = null, string btnStr = "确 定")
    {
        if (commonTipPanel != null)
        {
            commonTipPanel.ShowTip(content, onConfirm, btnStr);
        }
        else
        {
            Debug.LogError("GlobalCanvas: CommonTipPanel 未赋值，无法显示弹窗！");
        }
    }
}