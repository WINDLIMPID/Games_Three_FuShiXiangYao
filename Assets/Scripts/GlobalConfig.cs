using UnityEngine;

public class GlobalConfig : MonoBehaviour
{
    public static GlobalConfig Instance;

    [Header("=== 全局关卡总表 (新功能) ===")]
    public LevelConfigTable levelTable;

    [Header("=== 玩家初始属性 ===")]
    public int initialMaxHealth = 100;
    public float initialMoveSpeed = 5.0f;
    public float initialExpMultiplier = 1.0f;

    [Header("=== 符文初始属性 ===")]
    public int initialTalismanCount = 1;
    public float initialRotateSpeed = 180f;
    public float initialRadius = 2.0f;

    // --- 运行时数据 ---
    [HideInInspector]
    public LevelConfigEntry currentLevelConfig;
    [HideInInspector]
    public int currentLevelIndex = 1;

    // 🔥🔥 新增：记录是否应该默认打开选关界面
    // 如果为 true，下次加载 MainMenu 场景时，会自动打开 ChooseLevelUI
    [HideInInspector]
    public bool isLevelSelectionOpen = false;

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
}