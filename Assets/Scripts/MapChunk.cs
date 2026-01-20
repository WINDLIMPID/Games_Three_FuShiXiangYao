using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class MapChunk : MonoBehaviour
{
    [Header("配置")]
    public List<LevelButton> levelButtons;

    public void SetupChunk(int startLevelIndex, int totalLevelCount, Action<int, LevelButton> onLevelClick)
    {
        int unlockedLevel = 1;
        if (SaveManager.Instance != null)
            unlockedLevel = SaveManager.Instance.GetUnlockedLevel();

        var allLevelConfigs = GlobalConfig.Instance.levelTable.allLevels;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            // startLevelIndex 是这页第1个关卡的ID (1, 6, 11...)
            int currentLevelNum = startLevelIndex + i;

            if (currentLevelNum <= totalLevelCount)
            {
                levelButtons[i].gameObject.SetActive(true);

                LevelConfigEntry data = null;
                if (currentLevelNum - 1 < allLevelConfigs.Count)
                {
                    data = allLevelConfigs[currentLevelNum - 1];
                }

                // 1. 判断是否解锁
                bool isUnlocked = currentLevelNum <= unlockedLevel;

                // 2. 🔥 关键修正：LevelButton 需要的是 isLocked (是否锁定)
                // 所以这里要传 !isUnlocked (如果不解锁，那就是锁定)
                bool isLocked = !isUnlocked;

                // 3. 传递给按钮 (注意参数顺序：ID, 数据, 是否锁定, 回调)
                levelButtons[i].Setup(currentLevelNum, data, isLocked, onLevelClick);
            }
            else
            {
                levelButtons[i].gameObject.SetActive(false);
            }
        }
    }
}