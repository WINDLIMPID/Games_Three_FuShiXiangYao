using UnityEngine;
using System.Collections.Generic;

public class DataGenerator : MonoBehaviour
{
    [Header("1. 把 LevelConfigTable 拖进来")]
    public LevelConfigTable targetTable;

    [Header("2. 把 怪物预制体 拖进来")]
    public GameObject[] enemyPrefabs;

    // 点击 Inspector 右上角三个点 -> GenerateLevels 执行
    [ContextMenu("🔥 一键生成8关数据")]
    public void GenerateLevels()
    {
        if (targetTable == null || enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("❌ 请先拖入 LevelConfigTable 和 敌人预制体！");
            return;
        }

        targetTable.allLevels.Clear();

        // 🔥🔥🔥 核心：配置每一关对应 EnemyCreatPoint 下面的哪个子物体 🔥🔥🔥

        // 第1关 -> 用 Map1Point
        AddLevel(1, "境界一·炼气入体", 30f, 1.0f, 2.0f, "Map1Point");

        // 第2关 -> 用 Map2Point (如果你还没做Map2Point，这里先填 Map1Point 也可以)
        AddLevel(2, "境界二·筑基初成", 45f, 1.5f, 1.8f, "Map1Point");

        // 后面的关卡以此类推...
        AddLevel(3, "境界三·金丹大道", 60f, 2.0f, 1.5f, "Map1Point");
        AddLevel(4, "境界四·元婴化神", 90f, 3.0f, 1.2f, "Map1Point");
        AddLevel(5, "境界五·出窍神游", 120f, 4.0f, 1.0f, "Map1Point");
        AddLevel(6, "境界六·合体归一", 150f, 6.0f, 0.8f, "Map1Point");
        AddLevel(7, "境界七·渡劫飞升", 180f, 8.0f, 0.5f, "Map1Point");
        AddLevel(8, "境界八·大乘圆满", 300f, 10.0f, 0.3f, "Map1Point");

        Debug.Log("✅ 关卡数据已生成！记得保存 Project。");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetTable);
#endif
    }

    void AddLevel(int index, string title, float duration, float hpMult, float baseInterval, string mapNodeName)
    {
        LevelConfigEntry level = new LevelConfigEntry();

        level.displayTitle = title;
        level.surviveDuration = duration;

        // 🔥 赋值名字
        level.spawnPointGroupName = mapNodeName;

        level.enemyHpMultiplier = hpMult;
        level.enemySpeedMultiplier = 1.0f + (index * 0.05f);
        level.expGainMultiplier = Mathf.Max(1.0f, hpMult * 0.8f);

        level.waves = new List<EnemyWave>();

        // 简单生成3个阶段
        float[] times = { 0f, duration * 0.3f, duration * 0.8f };
        float[] intervals = { baseInterval * 1.5f, baseInterval, baseInterval * 0.5f };
        string[] names = { "试探", "围攻", "死斗" };

        for (int i = 0; i < 3; i++)
        {
            EnemyWave wave = new EnemyWave();
            wave.waveName = names[i];
            wave.startTime = times[i];
            wave.spawnInterval = intervals[i];
            wave.prefabs = enemyPrefabs;
            level.waves.Add(wave);
        }

        targetTable.allLevels.Add(level);
    }
}