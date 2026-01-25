using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryUI : MonoBehaviour
{
    public GameObject victoryPanel;
    public TextMeshProUGUI rewardText;
    public Button backButton;
    public Button restartButton;
    public Button nextButton;

    private int _finishedLevelIndex = 1;

    void Start()
    {
        if (victoryPanel) victoryPanel.SetActive(false);

        // 使用封装方法，简化逻辑
        if (backButton) backButton.onClick.AddListener(LoadMainMenu);
        if (restartButton) restartButton.onClick.AddListener(ReloadScene);
        if (nextButton) nextButton.onClick.AddListener(OnNextLevelClicked);
    }

    public void ShowVictory(int levelIndex, int killCount)
    {
        _finishedLevelIndex = levelIndex;

        int baseReward = levelIndex * 50;
        int killReward = killCount * 2;
        int totalGold = baseReward + killReward;

        if (rewardText != null) rewardText.text = $"金元宝 x{totalGold}";
        if (SaveManager.Instance != null) SaveManager.Instance.AddMoney(totalGold);

        bool hasNext = true;
        if (GlobalConfig.Instance?.levelTable != null)
        {
            if (levelIndex > 0 && levelIndex >= GlobalConfig.Instance.levelTable.allLevels.Count) hasNext = false;
        }
        if (nextButton) nextButton.gameObject.SetActive(hasNext);

        if (victoryPanel) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        int nextIndex = _finishedLevelIndex + 1;
        if (GlobalConfig.Instance != null)
        {
            GlobalConfig.Instance.currentLevelIndex = nextIndex;
            if (GlobalConfig.Instance.levelTable && nextIndex <= GlobalConfig.Instance.levelTable.allLevels.Count)
                GlobalConfig.Instance.currentLevelConfig = GlobalConfig.Instance.levelTable.allLevels[nextIndex - 1];
        }
        // 🔥 进战斗
        LoadBattleScene();
    }

    // 🔥🔥🔥 统一场景加载逻辑 🔥🔥🔥

    void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (SceneController.Instance) SceneController.Instance.LoadMainMenu();
        else SceneManager.LoadScene("MainMenuScene");
    }

    void ReloadScene()
    {
        Time.timeScale = 1f;
        if (SceneController.Instance) SceneController.Instance.ReloadCurrentScene();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void LoadBattleScene()
    {
        Time.timeScale = 1f;
        if (SceneController.Instance) SceneController.Instance.LoadBattle();
        else SceneManager.LoadScene("BattleScene");
    }
}