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
    public UI_EndlessSettlement endScoreUI;

    [Header("=== 核心设置 ===")]
    public Transform[] spawnPoints;
    public GameObject playerManagerPrefab;

    [Header("=== 战斗 UI - HUD 显示 ===")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI scoreText;

    [Header("=== 战斗 UI - 按钮 ===")]
    public Button settingsButton;
    public Button homeButton;
    public Button replayButton;

    [Header("=== 关卡数据 ===")]
    public int currentLevelIndex = 1;
    public float levelWinTime = 60f;
    public List<EnemyWave> waves = new List<EnemyWave>();
    public int maxEnemyCount = 300;

    [Header("无尽模式状态")]
    public int currentScore = 0;
    public bool isSpawningPaused = false;

    // 内部变量
    private float _gameTime = 0f;
    private float _spawnTimer = 0f;
    private int _currentWaveIndex = 0;
    private bool _isLevelFinished = false;
    private Transform _playerTransform;

    public bool IsEndlessMode => levelWinTime > 36000f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (victoryUI == null) victoryUI = FindObjectOfType<VictoryUI>();

        // 读取配置
        string targetMapName = "Map1Point";
        if (GlobalConfig.Instance != null && GlobalConfig.Instance.currentLevelConfig != null)
        {
            var config = GlobalConfig.Instance.currentLevelConfig;
            this.waves = config.waves;
            this.levelWinTime = config.surviveDuration;
            this.currentLevelIndex = GlobalConfig.Instance.currentLevelIndex;
            if (stageNameText != null) stageNameText.text = config.displayTitle;

            if (!string.IsNullOrEmpty(config.spawnPointGroupName))
            {
                targetMapName = config.spawnPointGroupName;
            }
        }

        // 仅无尽模式显示分数
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(IsEndlessMode);
            if (IsEndlessMode) scoreText.text = "击杀: 0";
        }

        FindSpawnPointsAndSetupPlayer(targetMapName);
        BindFunctionButtons();

        if (PlayerPrefs.GetInt("IsTutorialFinished", 0) == 0)
        {
            isSpawningPaused = true;
        }
    }

    public void StartSpawning() => isSpawningPaused = false;

    public void AddScore(int amount = 1)
    {
        if (IsEndlessMode)
        {
            currentScore += amount;
            if (scoreText != null) scoreText.text = "击杀: " + currentScore;
        }
    }

    // =========================================================
    // 🔥🔥🔥 核心修复：彻底打扫战场 (怪 + 经验球)
    // =========================================================
    public void ClearBattlefield()
    {
        // 1. 清理敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Despawn(enemy);
            else Destroy(enemy);
        }

        // 2. 🔥🔥 关键：清理所有经验球！防止在那1秒等待期内吃到经验升级
        ExpGem[] gems = FindObjectsOfType<ExpGem>();
        foreach (var gem in gems)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Despawn(gem.gameObject);
            else Destroy(gem.gameObject);
        }

        // 3. 🔥🔥 保底：如果有升级弹窗正在显示，强制关掉
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.levelUpPanel != null)
        {
            if (UpgradeManager.Instance.levelUpPanel.activeSelf)
            {
                UpgradeManager.Instance.levelUpPanel.SetActive(false);
                Time.timeScale = 1f; // 恢复时间，避免卡死
            }
        }

        Debug.Log($"🧹 战场大扫除完毕 (清除敌人: {enemies.Length}, 经验球: {gems.Length})");
    }

    // =========================================================
    // 按钮逻辑
    // =========================================================
    void BindFunctionButtons()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => {
                if (GlobalCanvas.Instance != null) GlobalCanvas.Instance.ToggleSettings();
            });
        }

        // 🔥 Home 按钮逻辑
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => {

                _isLevelFinished = true; // 锁定状态
                ClearBattlefield();      // 🔥 瞬间清空所有东西

                if (IsEndlessMode)
                {
                    StartCoroutine(DelayedEndlessOverProcess());
                }
                else
                {
                    StartCoroutine(DelayedBackHomeProcess());
                }
            });
        }

        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() => {
                Time.timeScale = 1f;
                ClearBattlefield(); // 重玩也要清
                if (SceneController.Instance != null)
                    SceneController.Instance.ReloadCurrentScene();
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
    }

    IEnumerator DelayedBackHomeProcess()
    {
        // 安全的1秒留白
        yield return new WaitForSecondsRealtime(1.0f);

        Time.timeScale = 1f;
        if (SceneController.Instance != null)
            SceneController.Instance.LoadMainMenu();
        else
            SceneManager.LoadScene("MainMenuScene");
    }

    // =========================================================
    // 游戏循环
    // =========================================================
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
            timerText.text = $"<color=#FFD700>∞ {m:00}:{s:00}</color>";
            timerText.transform.localScale = Vector3.one;
        }
        else
        {
            float remaining = Mathf.Max(0, levelWinTime - _gameTime);
            int m = Mathf.FloorToInt(remaining / 60F);
            int s = Mathf.FloorToInt(remaining % 60F);
            timerText.text = $"{m:00}:{s:00}";

            // 倒计时呼吸特效
            if (remaining <= 5f)
            {
                timerText.color =new Color32(142,0,0,255);
                float pulseSpeed = (remaining <= 5f) ? 1 : 0.5f;
                float scale = 1.0f + Mathf.PingPong(Time.time * pulseSpeed, 0.2f);
                timerText.transform.localScale = Vector3.one * scale;
            }
            else
            {
                timerText.color = Color.white;
                timerText.transform.localScale = Vector3.one;
            }
        }
    }

    // =========================================================
    // 胜利逻辑
    // =========================================================
    void WinGame()
    {
        _isLevelFinished = true;
        Debug.Log("🎉 时间到！清理战场...");

        // 1. 瞬间清怪 + 清经验球
        ClearBattlefield();

        // 2. 等待1秒再出结算
        StartCoroutine(DelayedWinProcess());
    }

    IEnumerator DelayedWinProcess()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        if (SaveManager.Instance != null) SaveManager.Instance.CompleteLevel(currentLevelIndex);
        if (victoryUI != null) victoryUI.ShowVictory(currentLevelIndex);
    }
    // =========================================================
    // 🔥 修复：无尽模式结算逻辑 (新公式版)
    // =========================================================
    public void OnEndlessModeGameOver()
    {
        _isLevelFinished = true;

        // 1. 清理战场
        ClearBattlefield();

        // 2. 启动结算流程
        StartCoroutine(DelayedEndlessOverProcess());
    }

    System.Collections.IEnumerator DelayedEndlessOverProcess()
    {
        // 稍等一秒，给玩家喘息时间
        yield return new WaitForSecondsRealtime(1.0f);

        // --- 📊 数据计算区域 ---

        // 1. 获取基础数据
        int killCount = currentScore; // 这里的 currentScore 实际上是击杀数 (AddScore(1) 加上去的)
        int playTimeSeconds = Mathf.FloorToInt(_gameTime);

        // 2. 🔥 应用新公式：分数 = 击杀数x100 + 时间(秒)x50
        int finalScore = (killCount * 100) + (playTimeSeconds * 50);

        Debug.Log($"💀 结算: 击杀{killCount} | 时间{playTimeSeconds}s | 总分{finalScore}");

        // 3. 处理存档 (比对 FinalScore)
        int bestScore = finalScore;
        bool isNewRecord = false;

        if (SaveManager.Instance != null)
        {
            // 尝试保存新的【总分】作为最高分
            isNewRecord = SaveManager.Instance.TrySaveHighScore(finalScore);
            bestScore = SaveManager.Instance.GetHighScore();
        }

        // 4. 发放金币 (可选：如果你想按分数发钱，比如 100分=1金币)
        // int coins = Mathf.FloorToInt(finalScore / 100f);
        // if (MoneyManager.Instance != null) MoneyManager.Instance.AddCoins(coins);

        // --- 🖥️ UI 显示区域 ---

        if (endScoreUI != null)
        {
            // 参数顺序：总分, 最高分, 新纪录?, 用时, 降妖数
            endScoreUI.Show(finalScore, bestScore, isNewRecord, _gameTime, killCount);
        }

        if (LeaderboardSystem.Instance != null)
        {
            LeaderboardSystem.Instance.RefreshLeaderboard();
        }
    }

    // =========================================================
    // 刷怪辅助 (保持不变)
    // =========================================================
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

        if (PoolManager.Instance != null)
            PoolManager.Instance.Spawn(prefab, pos, Quaternion.identity);
        else
            Instantiate(prefab, pos, Quaternion.identity);
    }

    Vector3 GetBestSpawnPosition()
    {
        if (_playerTransform == null && spawnPoints.Length > 0) return spawnPoints[0].position;
        if (_playerTransform == null) return Vector3.zero;

        var sortedPoints = spawnPoints.OrderBy(p => Vector3.Distance(p.position, _playerTransform.position)).ToList();
        int startIndex = Mathf.FloorToInt(sortedPoints.Count * 0.5f);
        int randomIndex = Random.Range(startIndex, sortedPoints.Count);

        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        return sortedPoints[randomIndex].position + offset;
    }

    void FindSpawnPointsAndSetupPlayer(string mapNodeName)
    {
        GameObject topRoot = GameObject.Find("EnemyCreatPoint");
        if (topRoot == null)
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer) _playerTransform = existingPlayer.transform;
            return;
        }
        Transform mapRoot = topRoot.transform.Find(mapNodeName);
        if (mapRoot == null) { if (topRoot.transform.childCount > 0) mapRoot = topRoot.transform.GetChild(0); else return; }

        Transform enemyContainer = mapRoot.Find("EnemyPoint");
        if (enemyContainer != null)
        {
            spawnPoints = new Transform[enemyContainer.childCount];
            for (int i = 0; i < enemyContainer.childCount; i++) spawnPoints[i] = enemyContainer.GetChild(i);
        }

        Transform playerContainer = mapRoot.Find("PlayerPoint");
        if (playerContainer != null && playerContainer.childCount > 0)
        {
            Transform startPoint = playerContainer.GetChild(0);
            GameObject oldManager = GameObject.Find("PlayerManager");
            if (oldManager != null) Destroy(oldManager);

            if (playerManagerPrefab != null)
            {
                GameObject newManager = Instantiate(playerManagerPrefab, startPoint.position, Quaternion.identity);
                newManager.name = "PlayerManager";
                Transform realPlayer = newManager.transform.Find("Player");
                if (realPlayer != null) _playerTransform = realPlayer;
                else foreach (Transform child in newManager.transform) if (child.CompareTag("Player")) { _playerTransform = child; break; }
            }
        }
    }
}