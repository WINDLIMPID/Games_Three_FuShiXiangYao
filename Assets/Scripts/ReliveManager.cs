using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReliveManager : MonoBehaviour
{
    public static ReliveManager Instance;

    public GameObject relivePanel;
    public GameObject failPanel;

    public Button adReviveBtn;
    public Button giveUpBtn;

    // 失败面板上的按钮
    public Button failHomeBtn;
    public Button failRestartBtn;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (relivePanel) relivePanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);

        if (adReviveBtn) adReviveBtn.onClick.AddListener(OnAdReviveClicked);
        if (giveUpBtn) giveUpBtn.onClick.AddListener(OnGiveUpClicked);

        // 🔥🔥🔥 核心修改：这里全部改用 SceneController 了！ 🔥🔥🔥
        if (failHomeBtn) failHomeBtn.onClick.AddListener(() => {
            Time.timeScale = 1f;
            if (SceneController.Instance) SceneController.Instance.LoadMainMenu();
            else SceneManager.LoadScene("MainMenuScene");
        });

        if (failRestartBtn) failRestartBtn.onClick.AddListener(() => {
            Time.timeScale = 1f;
            if (SceneController.Instance) SceneController.Instance.ReloadCurrentScene();
            else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    public void ShowRelivePanel()
    {
        if (relivePanel)
        {
            relivePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void OnAdReviveClicked()
    {
        Debug.Log("看广告复活成功！");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            Health hp = player.GetComponent<Health>();
            if (hp) hp.Resurrect();
        }
        ReviveCleanup();
    }

    void ReviveCleanup()
    {
        Time.timeScale = 1f;
        if (relivePanel) relivePanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
    }

    void OnGiveUpClicked()
    {
        if (relivePanel) relivePanel.SetActive(false);

        // 检查是否是无尽模式，调用修复后的方法
        if (EnemySpawner.Instance != null && EnemySpawner.Instance.IsEndlessMode)
        {
            EnemySpawner.Instance.OnEndlessModeGameOver();
        }
        else
        {
            if (failPanel) failPanel.SetActive(true);
        }
    }
}