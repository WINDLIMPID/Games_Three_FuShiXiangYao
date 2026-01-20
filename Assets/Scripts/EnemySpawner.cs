using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("=== 核心引用 ===")]
    // 🔥 新增：引用新的 UI 控制脚本
    public VictoryUI victoryUI;

    [Header("=== 核心设置 (自动读取) ===")]
    public Transform[] spawnPoints;

    [Header("=== 玩家生成设置 ===")]
    public GameObject playerManagerPrefab;

    [Header("=== 战斗 UI - HUD 显示 ===")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stageNameText;

    // 注意：原本的 victoryPanel, backButton, nextButton 等都被移除了
    // 因为它们现在由 VictoryUI 脚本接管

    [Header("=== 战斗 UI - 顶部功能按钮 (HUD) ===")]
    public Button settingsButton; // ⚙️ 设置按钮
    public Button homeButton;     // 🏠 主页按钮
    public Button replayButton;   // 🔄 重玩按钮

    [Header("=== 关卡设置 (自动读取) ===")]
    public int currentLevelIndex = 1;
    public float levelWinTime = 60f;

    [Header("波次配置")]
    public List<EnemyWave> waves = new List<EnemyWave>();
    public int maxEnemyCount = 300;

    // --- 内部变量 ---
    private float _gameTime = 0f;
    private float _spawnTimer = 0f;
    private int _currentWaveIndex = 0;
    private bool _isLevelFinished = false;
    private Transform _playerTransform;

    void Start()
    {
        // 0. 自动查找引用 (防止忘记拖拽)
        if (victoryUI == null)
        {
            victoryUI = FindObjectOfType<VictoryUI>();
        }

        // 1. 初始化配置
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

        // 2. 找点位并生成玩家
        FindSpawnPointsAndSetupPlayer(targetMapName);

        // 3. 绑定 HUD 功能按钮 (设置、主页、重玩)
        BindFunctionButtons();

        // 4. (已删除) BindVictoryButtons() -> 逻辑已移交 VictoryUI
    }

    // =========================================================
    // 绑定顶部功能按钮 (Settings, Home, Replay) - 这些还在 HUD 上
    // =========================================================
    void BindFunctionButtons()
    {
        // 1. 设置按钮
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => {
                if (GlobalCanvas.Instance != null)
                {
                    GlobalCanvas.Instance.ToggleSettings();
                }
                else
                {
                    Debug.LogWarning("场景中没有 GlobalCanvas，无法打开设置面板！");
                }
            });
        }

        // 2. 主页按钮
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => {
                Time.timeScale = 1f; // 确保时间恢复
                if (SceneController.Instance != null)
                    SceneController.Instance.LoadMainMenu();
                else
                    SceneManager.LoadScene("MainMenuScene");
            });
        }

        // 3. 重玩按钮
        if (replayButton != null)
        {
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() => {
                Time.timeScale = 1f; // 确保时间恢复
                if (SceneController.Instance != null)
                    SceneController.Instance.ReloadCurrentScene();
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
    }

    // =========================================================
    // 核心逻辑：寻找点位 & 生成玩家
    // =========================================================
    void FindSpawnPointsAndSetupPlayer(string mapNodeName)
    {
        GameObject topRoot = GameObject.Find("EnemyCreatPoint");
        if (topRoot == null)
        {
            Debug.LogError("❌ 找不到 [EnemyCreatPoint]！");
            return;
        }

        Transform mapRoot = topRoot.transform.Find(mapNodeName);
        if (mapRoot == null)
        {
            Debug.LogError($"❌ 找不到子物体 [{mapNodeName}]！");
            return;
        }

        // 怪物点位
        Transform enemyContainer = mapRoot.Find("EnemyPoint");
        if (enemyContainer != null)
        {
            int count = enemyContainer.childCount;
            spawnPoints = new Transform[count];
            for (int i = 0; i < count; i++)
                spawnPoints[i] = enemyContainer.GetChild(i);
        }

        // 玩家生成
        Transform playerContainer = mapRoot.Find("PlayerPoint");
        if (playerContainer != null && playerContainer.childCount > 0)
        {
            Transform startPoint = playerContainer.GetChild(0);
            Vector3 spawnPos = startPoint.position;

            GameObject oldManager = GameObject.Find("PlayerManager");
            if (oldManager != null) Destroy(oldManager);

            if (playerManagerPrefab != null)
            {
                GameObject newManager = Instantiate(playerManagerPrefab, spawnPos, Quaternion.identity);
                newManager.name = "PlayerManager";

                Transform realPlayer = newManager.transform.Find("Player");
                if (realPlayer != null)
                {
                    _playerTransform = realPlayer;
                }
                else
                {
                    foreach (Transform child in newManager.transform)
                    {
                        if (child.CompareTag("Player")) { _playerTransform = child; break; }
                    }
                }
            }
        }
    }

    void Update()
    {
        if (_isLevelFinished) return;
        if (_playerTransform == null) return;

        // 检查玩家血量
        var playerHealth = _playerTransform.GetComponent<Health>();
        if (playerHealth != null && playerHealth.currentHealth <= 0) return;

        // 1. 计时
        _gameTime += Time.deltaTime;
        UpdateTimerUI();

        // 2. 胜利判定
        if (_gameTime >= levelWinTime)
        {
            WinGame();
            return;
        }

        // 3. 波次控制
        if (_currentWaveIndex < waves.Count - 1)
        {
            if (_gameTime >= waves[_currentWaveIndex + 1].startTime)
                _currentWaveIndex++;
        }

        // 4. 刷怪
        UpdateSpawning();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 判定：如果目标时间特别大，说明是“无尽模式”
            if (levelWinTime > 9999)
            {
                // 🔥🔥🔥 修改核心：无尽模式显示“已生存时间” (正向计时)

                // 计算游戏已经进行了多久 (_gameTime)
                int minutes = Mathf.FloorToInt(_gameTime / 60F);
                int seconds = Mathf.FloorToInt(_gameTime % 60F);

                // 显示格式：无限符号 + 时间 (例如: ∞ 05:30)
                // 我加了个金色 (<color=#FFD700>) 让它看起来比较特别
                timerText.text = string.Format("<color=#FFD700>∞ {0:00}:{1:00}</color>", minutes, seconds);
            }
            else
            {
                // --- 剧情模式：保持原有的倒计时逻辑 ---
                float remainingTime = Mathf.Max(0, levelWinTime - _gameTime);
                int minutes = Mathf.FloorToInt(remainingTime / 60F);
                int seconds = Mathf.FloorToInt(remainingTime % 60F);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                // 最后 5 秒变红警告
                if (remainingTime <= 5f) timerText.color = Color.red;
                else timerText.color = Color.white;
            }
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
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Vector3 finalSpawnPos = GetBestSpawnPosition();
        GameObject randomPrefab = wave.prefabs[Random.Range(0, wave.prefabs.Length)];

        // 使用对象池生成
        if (PoolManager.Instance != null)
            PoolManager.Instance.Spawn(randomPrefab, finalSpawnPos, Quaternion.identity);
        else
            Instantiate(randomPrefab, finalSpawnPos, Quaternion.identity);
    }

    Vector3 GetBestSpawnPosition()
    {
        if (_playerTransform == null) return spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        // 简单的距离排序：选离玩家远的
        var sortedPoints = spawnPoints.OrderBy(p => Vector3.Distance(p.position, _playerTransform.position)).ToList();

        int totalCount = sortedPoints.Count;
        int startIndex = Mathf.FloorToInt(totalCount * 0.5f);
        if (startIndex >= totalCount) startIndex = totalCount - 1;

        int randomIndex = Random.Range(startIndex, totalCount);
        Transform chosenPoint = sortedPoints[randomIndex];

        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        return chosenPoint.position + offset;
    }

    // =========================================================
    // 胜利逻辑 (已重构)
    // =========================================================
    void WinGame()
    {
        _isLevelFinished = true;
        Debug.Log("🎉 关卡胜利！时间到达。");

        // 1. 保存通关记录
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CompleteLevel(currentLevelIndex);
        }

        // 2. 清理场上所有怪物
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Despawn(enemy);
            else Destroy(enemy);
        }

        // 3. 🔥 调用 VictoryUI 显示界面 (核心修改)
        if (victoryUI != null)
        {
            victoryUI.ShowVictory(currentLevelIndex);
        }
        else
        {
            Debug.LogError("❌ EnemySpawner: 找不到 VictoryUI 引用，无法弹出胜利界面！");
            // 最后的保底，防止卡死
            Time.timeScale = 0f;
        }
    }

    void OnDrawGizmos()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var point in spawnPoints)
            {
                if (point != null) Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}