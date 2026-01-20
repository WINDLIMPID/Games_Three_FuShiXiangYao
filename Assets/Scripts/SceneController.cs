using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("配置场景名字 (改名只用改这里)")]
    [Tooltip("主菜单场景的名字")]
    public string menuSceneName = "MenuScene";

    [Tooltip("战斗核心场景的名字")]
    public string battleSceneName = "MainScene";

    [Header("过渡动画 (可选)")]
    public CanvasGroup fadeCanvasGroup; // 拖入一个全屏黑色的Panel
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保证切场景时不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // 切场景前必须恢复时间！
        StartCoroutine(TransitionToScene(menuSceneName));
    }

    /// <summary>
    /// 进入战斗场景 (新游戏/下一关/重开)
    /// </summary>
    public void LoadBattle()
    {
        Time.timeScale = 1f;
        StartCoroutine(TransitionToScene(battleSceneName));
    }

    /// <summary>
    /// 重新加载当前场景 (原地复活)
    /// </summary>
    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        string currentName = SceneManager.GetActiveScene().name;
        StartCoroutine(TransitionToScene(currentName));
    }

    // 统一的加载协程 (带简单的淡入淡出)
    private IEnumerator TransitionToScene(string sceneName)
    {
        // 1. 淡出 (变黑)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true; // 阻挡点击
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                yield return null;
            }
        }

        // 2. 真正加载场景
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // 3. 淡入 (变亮)
        if (fadeCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.blocksRaycasts = false; // 恢复点击
        }
    }
}