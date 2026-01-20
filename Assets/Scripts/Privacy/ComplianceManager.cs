using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ComplianceManager : MonoBehaviour
{
    [Header("配置")]
    public float displayTime = 5.0f; // 必须设为5秒或以上
    public string nextSceneName = "MainMenu"; // 健康忠告完后跳转的场景

    private void Start()
    {
        // 开启协程开始倒计时
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        // 强制等待，不受 Time.timeScale 影响
        yield return new WaitForSecondsRealtime(displayTime);

        // 跳转场景
        SceneManager.LoadScene(nextSceneName);
    }
}