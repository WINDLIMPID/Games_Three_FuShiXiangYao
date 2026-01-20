using UnityEngine;

public class GlobalCanvas : MonoBehaviour
{
    public static GlobalCanvas Instance;

    [Header("挂载界面")]
    public GameObject settingsPanel; // 整个设置界面的父物体

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
        // 游戏开始时确保关闭
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // 提供给按钮调用 (切换开关)
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            // 取反：当前是开就关，当前是关就开
            bool isOpening = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isOpening);

            // 🔥 核心逻辑：开界面就暂停(0)，关界面就恢复(1)
            Time.timeScale = isOpening ? 0f : 1f;
        }
    }

    // 专门给“空白背景”用的关闭方法
    public void CloseSettings()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);

            // 🔥 恢复游戏
            Time.timeScale = 1f;
        }
    }
}