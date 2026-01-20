using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_LevelUp_Panel : MonoBehaviour
{
    [Header("卡片数据")]
    public UI_SkillCard[] skillCards;

    [Tooltip("请把包含那3张卡片的父物体(Container)拖到这里，不要拖整个界面！")]
    [Header("动画设置")]
    public Transform cardsContainer;

    // 显示面板
    public void Show()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f; // 暂停游戏

        // 🔥 只对卡片容器做果冻动画
        if (cardsContainer != null)
        {
            cardsContainer.localScale = Vector3.zero; // 先设为0

            cardsContainer.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack) // 强力回弹
                .SetUpdate(true);      // 忽略暂停
        }
    }

    // 隐藏面板
    public void Hide()
    {
        // 升级选完后通常希望立刻恢复战斗，所以这里我不加关闭动画了，直接关
        // 如果你想加，参考 SimpleWindowUI 的写法即可

        Time.timeScale = 1f; // 恢复游戏
        gameObject.SetActive(false);
    }
}