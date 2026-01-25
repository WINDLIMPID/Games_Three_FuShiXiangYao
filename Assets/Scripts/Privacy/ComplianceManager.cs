using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ComplianceManager : MonoBehaviour
{
    [Header("配置")]
    public float displayTime = 5.0f; // 必须设为5秒或以上
    public string nextSceneName = "MainMenuScene";

    private void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        // 强制等待，不受 Time.timeScale 影响
        yield return new WaitForSecondsRealtime(displayTime);

        // 🔥🔥🔥 统一使用 SceneController (如果有的话) 🔥🔥🔥
        if (SceneController.Instance != null)
        {
            // 这里因为是从健康忠告跳转，可以用通用方法，也可以直接跳
            // 假设 LoadMainMenu() 就是去 MainMenuScene
            SceneController.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}