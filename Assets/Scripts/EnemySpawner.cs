using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("=== 核心引用 ===")]
    public VictoryUI victoryUI;
    public UI_EndlessSettlement endScoreUI; // 无尽模式结算界面

    [Header("=== 核心设置 ===")]
    public Transform[] spawnPoints;
    public GameObject playerManagerPrefab;

    [Header("=== 战斗 UI ===")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI scoreText;
    public Button settingsButton;
    public Button homeButton;
    public Button replayButton;

    [Header("=== 关卡数据 ===")]
    public int currentLevelIndex = 1;
    public float levelWinTime = 60f;
    public List<EnemyWave> waves = new List<EnemyWave>();
    public int maxEnemyCount = 300;

    [Header("状态")]
    public int currentScore = 0;

    // 默认暂停
    public bool isSpawningPaused = true;

    private float _gameTime = 0f;
    private float _spawnTimer = 0f;
    private int _currentWaveIndex = 0;
    private bool _isLevelFinished = false;
    private Transform _playerTransform;

    public bool IsEndlessMode => levelWinTime > 36000f;
    public int killCount = 0;

    void Awake()
    {
        Instance = this;
        isSpawningPaused = true;
    }

    void Start()
    {
        if (victoryUI == null) victoryUI = FindObjectOfType<VictoryUI>();
        if (endScoreUI == null) endScoreUI = FindObjectOfType<UI_EndlessSettlement>();

        string targetMapName = "Map1Point";
        if (GlobalConfig.Instance != null && GlobalConfig.Instance.currentLevelConfig != null)
        {
            var config = GlobalConfig.Instance.currentLevelConfig;
            this.waves = config.waves;
            this.levelWinTime = config.surviveDuration;
            this.currentLevelIndex = GlobalConfig.Instance.currentLevelIndex;
            if (stageNameText != null) stageNameText.text = config.displayTitle;
            if (!string.IsNullOrEmpty(config.spawnPointGroupName)) targetMapName = config.spawnPointGroupName;
        }

        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(IsEndlessMode);
            if (IsEndlessMode) scoreText.text = "降妖：0";
        }

        FindSpawnPointsAndSetupPlayer(targetMapName);
        BindFunctionButtons();
        UpdateTimerUI();

        isSpawningPaused = true;
    }

    public void StartSpawning()
    {
        isSpawningPaused = false;
        Debug.Log("💀 敌人生成器启动！开始刷怪 & 计时！");
    }

    // 无尽模式触发结算
    public void OnEndlessModeGameOver()
    {
        _isLevelFinished = true;
        Time.timeScale = 0f;
        ClearBattlefield();
        StartCoroutine(DelayedEndlessOverProcess());
    }

    public void AddScore(int amount = 1)
    {
        killCount += amount;
        if (IsEndlessMode)
        {
            currentScore += amount;
            if (scoreText != null) scoreText.text = "降妖：" + currentScore;
        }
    }

    public void ClearBattlefield()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Despawn(enemy);
            else Destroy(enemy);
        }
        ExpGem[] gems = FindObjectsOfType<ExpGem>();
        foreach (var gem in gems)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Despawn(gem.gameObject);
            else Destroy(gem.gameObject);
        }
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.levelUpPanel != null)
        {
            if (UpgradeManager.Instance.levelUpPanel.activeSelf)
            {
                UpgradeManager.Instance.levelUpPanel.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (_isLevelFinished || isSpawningPaused) return;

        if (_playerTransform == null) return;

        _gameTime += Time.deltaTime;
        UpdateTimerUI();

        if (!IsEndlessMode && _gameTime >= levelWinTime)
        {
            WinGame();
            return;
        }

        if (_currentWaveIndex < waves.Count - 1)
        {
            if (_gameTime >= waves[_currentWaveIndex + 1].startTime)
                _currentWaveIndex++;
        }

        UpdateSpawning();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        if (IsEndlessMode)
        {
            int m = Mathf.FloorToInt(_gameTime / 60F);
            int s = Mathf.FloorToInt(_gameTime % 60F);
            timerText.text = $"∞ {m:00}:{s:00}";
        }
        else
        {
            float remaining = Mathf.Max(0, levelWinTime - _gameTime);
            int m = Mathf.FloorToInt(remaining / 60F);
            int s = Mathf.FloorToInt(remaining % 60F);
            timerText.text = $"{m:00}:{s:00}";
        }
    }

    void WinGame()
    {
        _isLevelFinished = true;
        ClearBattlefield();
        StartCoroutine(DelayedWinProcess());
    }

    IEnumerator DelayedWinProcess()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        if (SaveManager.Instance != null) SaveManager.Instance.CompleteLevel(currentLevelIndex);
        if (victoryUI != null) victoryUI.ShowVictory(currentLevelIndex, killCount);
    }

    // 🔥🔥🔥 核心修复位置 🔥🔥🔥
    IEnumerator DelayedEndlessOverProcess()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (endScoreUI != null)
        {
            // 公式：(关卡数 * 50) + (击杀数 * 10) + (存活秒数 * 2)
            // 这里用 (波数+1) 代替关卡数
            int waveScore = (_currentWaveIndex + 1) * 50;
            int killScore = killCount * 10;
            int timeScore = Mathf.FloorToInt(_gameTime) * 2;

            int finalTotalScore = waveScore + killScore + timeScore;

            Debug.Log($"无尽结算公式：(波数{_currentWaveIndex + 1} * 50) + ({killCount} * 10) + ({Mathf.FloorToInt(_gameTime)} * 2) = {finalTotalScore}");

            int bestScore = 0;
            if (SaveManager.Instance != null)
            {
                bestScore = SaveManager.Instance.GetHighScore();
            }

            bool isNewRecord = false;
            // 如果破纪录了
            if (finalTotalScore > bestScore)
            {
                isNewRecord = true;
                bestScore = finalTotalScore;

                // 1. 保存到本地
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.TrySaveHighScore(finalTotalScore);
                }

                // 2. 🔥 强制刷新排行榜数据！🔥
                // 告诉 LeaderboardSystem：“存档变了，快重新读取一下！”
                if (LeaderboardSystem.Instance != null)
                {
                    LeaderboardSystem.Instance.RefreshLeaderboard();
                }
            }

            endScoreUI.Show(finalTotalScore, bestScore, isNewRecord, _gameTime, killCount);
        }
    }

    void UpdateSpawning()
    {
        if (waves.Count == 0 || EnemyAI.ActiveCount >= maxEnemyCount) return;
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= waves[_currentWaveIndex].spawnInterval)
        {
            SpawnEnemy(waves[_currentWaveIndex]);
            _spawnTimer = 0f;
        }
    }

    void SpawnEnemy(EnemyWave wave)
    {
        if (wave.prefabs == null || wave.prefabs.Length == 0) return;
        Vector3 pos = GetBestSpawnPosition();
        GameObject prefab = wave.prefabs[Random.Range(0, wave.prefabs.Length)];
        if (PoolManager.Instance != null) PoolManager.Instance.Spawn(prefab, pos, Quaternion.identity);
        else Instantiate(prefab, pos, Quaternion.identity);
    }

    Vector3 GetBestSpawnPosition()
    {
        if (_playerTransform == null && spawnPoints.Length > 0) return spawnPoints[0].position;
        if (_playerTransform == null) return Vector3.zero;

        var sortedPoints = spawnPoints.OrderBy(p => Vector3.Distance(p.position, _playerTransform.position)).ToList();
        int startIndex = Mathf.FloorToInt(sortedPoints.Count * 0.5f);
        int randomIndex = Random.Range(startIndex, sortedPoints.Count);
        return sortedPoints[randomIndex].position + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
    }

    void BindFunctionButtons()
    {
        if (settingsButton) settingsButton.onClick.AddListener(() => GlobalCanvas.Instance?.ToggleSettings());

        if (homeButton) homeButton.onClick.AddListener(() => {

            if (IsEndlessMode)
            {
                OnEndlessModeGameOver();
            }
            else
            {
                _isLevelFinished = true;
                ClearBattlefield();
                Time.timeScale = 1f;
                if (SceneController.Instance) SceneController.Instance.LoadMainMenu();
                else SceneManager.LoadScene("MainMenuScene");
            }
        });

        if (replayButton) replayButton.onClick.AddListener(() => {
            _isLevelFinished = true;
            Time.timeScale = 1f;
            ClearBattlefield();
            if (SceneController.Instance) SceneController.Instance.ReloadCurrentScene();
            else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    void FindSpawnPointsAndSetupPlayer(string mapNodeName)
    {
        GameObject topRoot = GameObject.Find("EnemyCreatPoint");
        if (topRoot == null) { GameObject pl = GameObject.FindGameObjectWithTag("Player"); if (pl) _playerTransform = pl.transform; return; }
        Transform mapRoot = topRoot.transform.Find(mapNodeName) ?? (topRoot.transform.childCount > 0 ? topRoot.transform.GetChild(0) : null);
        if (mapRoot == null) return;
        Transform ep = mapRoot.Find("EnemyPoint");
        if (ep) { spawnPoints = new Transform[ep.childCount]; for (int i = 0; i < ep.childCount; i++) spawnPoints[i] = ep.GetChild(i); }
        Transform pp = mapRoot.Find("PlayerPoint");
        if (pp && pp.childCount > 0 && playerManagerPrefab != null)
        {
            GameObject old = GameObject.Find("PlayerManager"); if (old) Destroy(old);
            GameObject nm = Instantiate(playerManagerPrefab, pp.GetChild(0).position, Quaternion.identity);
            nm.name = "PlayerManager";
            Transform real = nm.transform.Find("Player");
            if (real) _playerTransform = real;
        }
    }
}