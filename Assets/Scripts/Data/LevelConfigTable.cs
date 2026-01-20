using UnityEngine;
using System.Collections.Generic;

// 1. 波次定义
[System.Serializable]
public class EnemyWave
{
    public string waveName;      // 波次名字
    public float startTime;      // 开始时间
    public float spawnInterval;  // 刷新间隔
    public GameObject[] prefabs; // 怪物预制体
}

// 2. 单个关卡的数据结构
[System.Serializable]
public class LevelConfigEntry
{
    [Header("基础信息")]
    public string displayTitle = "境界一";
    public Sprite previewImage;

    // 🔥🔥🔥 这里存的是 EnemyCreatPoint 下面的子物体名字 (例如 Map1Point)
    public string spawnPointGroupName = "Map1Point";

    [Header("胜利与数值")]
    public float surviveDuration = 60f;
    public float enemyHpMultiplier = 1.0f;
    public float enemySpeedMultiplier = 1.0f;
    public float expGainMultiplier = 1.0f;

    [Header("刷怪配置")]
    public List<EnemyWave> waves = new List<EnemyWave>();
}

// 3. 总表
[CreateAssetMenu(fileName = "GameLevelTable", menuName = "Game/Level Config Table")]
public class LevelConfigTable : ScriptableObject
{
    public List<LevelConfigEntry> allLevels = new List<LevelConfigEntry>();
}