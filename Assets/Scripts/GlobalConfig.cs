using UnityEngine;

public class GlobalConfig : MonoBehaviour
{
    public static GlobalConfig Instance;

    [Header("=== 全局关卡总表 (新功能) ===")]
    public LevelConfigTable levelTable;

    // 🔥🔥🔥 恢复之前丢失的属性配置 🔥🔥🔥
    [Header("=== 玩家初始属性 ===")]
    public int initialMaxHealth = 100;
    public float initialMoveSpeed = 5.0f;
    public float initialExpMultiplier = 1.0f;

    [Header("=== 符文初始属性 ===")]
    public int initialTalismanCount = 1;
    public float initialRotateSpeed = 180f;
    public float initialRadius = 2.0f;

    // --- 运行时数据 (自动管理，不需要你在Inspector里填) ---
    [HideInInspector]
    public LevelConfigEntry currentLevelConfig;

    [HideInInspector]
    public int currentLevelIndex = 1;

    [HideInInspector]
    public bool isLevelSelectionOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 🔥 核心修复：切换场景时保留此物体，确保无尽模式的数据能带过去
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果场景里已经有了一个旧的（比如从主菜单带过来的），
            // 那么这个新场景里原本摆着的（空的）GlobalConfig 就必须销毁，
            // 否则它会覆盖掉我们带数据的那个！
            Destroy(gameObject);
        }
    }
}