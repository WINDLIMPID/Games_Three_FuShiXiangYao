using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class RankItemData
{
    public int rank;
    public string name;
    public int score;
    public bool isPlayer;
}

public class LeaderboardSystem : MonoBehaviour
{
    public static LeaderboardSystem Instance;

    private List<RankItemData> _cachedData;

    // 建议：如果你想排行榜人多一点，就在这里多加点名字，必须超过20个才满榜
    private string[] _fakeNames = new string[] {
        "清风散人", "云梦泽", "无极剑圣", "丹阳子", "九天玄女",
        "莫问天", "龙傲天", "一叶知秋", "狂刀长老", "青莲居士",
        "忘情尊者", "逍遥子", "玉面书生", "枯木道人", "赤练仙子",
        "虚空行者", "剑魔独孤", "不灭战神", "灵虚子", "踏雪无痕",
        "北凉徐凤年", "韩跑跑", "李淳罡", "王仙芝", "曹长卿"
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        List<RankItemData> tempChat = new List<RankItemData>();

        // 1. 获取玩家分数
        int playerScore = 0;
        if (SaveManager.Instance != null)
        {
            playerScore = SaveManager.Instance.GetHighScore();
        }

        // 2. 准备名字池 (核心修改：转为 List 以便删除已用的名字)
        List<string> namePool = new List<string>(_fakeNames);

        // 3. 确定生成数量
        // 目标是总共20人。因为有1个是玩家，所以假人最多生成 19 个。
        // 但是！如果名字库里只有 5 个名字，那就只生成 5 个，不强求。
        int maxFakeCount = 19;
        int loopCount = Mathf.Min(maxFakeCount, namePool.Count);

        int baseScore = playerScore > 0 ? playerScore : 1000;

        for (int i = 0; i < loopCount; i++)
        {
            RankItemData fake = new RankItemData();

            // 🔥 防重名核心逻辑：
            // 1. 随机选一个索引
            int randomIndex = Random.Range(0, namePool.Count);
            // 2. 取出名字
            fake.name = namePool[randomIndex];
            // 3. 马上从池子里删掉！下次就不会再抽到它了
            namePool.RemoveAt(randomIndex);

            fake.isPlayer = false;

            // 分数逻辑 (30%概率比玩家高，其余比玩家低)
            float rand = Random.value;
            if (rand < 0.3f) fake.score = (int)(baseScore * Random.Range(1.2f, 2.5f)) + Random.Range(100, 2000);
            else if (rand < 0.8f) fake.score = (int)(baseScore * Random.Range(0.8f, 1.1f));
            else fake.score = (int)(baseScore * Random.Range(0.1f, 0.7f));

            if (fake.score <= 0) fake.score = 10; // 保底分

            tempChat.Add(fake);
        }

        // 4. 插入玩家
        RankItemData player = new RankItemData();
        player.name = "我";
        player.score = playerScore;
        player.isPlayer = true;
        tempChat.Add(player);

        // 5. 排序 & 截取 & 标号
        tempChat = tempChat.OrderByDescending(x => x.score).ToList();

        // 重新整理缓存，最多取前20名 (防止名字库太大生成了几百个)
        _cachedData = new List<RankItemData>();
        int finalCount = Mathf.Min(tempChat.Count, 20);

        for (int i = 0; i < finalCount; i++)
        {
            var item = tempChat[i];
            item.rank = i + 1;
            _cachedData.Add(item);
        }
    }

    public List<RankItemData> GetLeaderboardData()
    {
        if (_cachedData == null) RefreshLeaderboard();
        return _cachedData;
    }
}