using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HUD_SkillSlot : MonoBehaviour
{
    [Header("组件引用")]
    public TextMeshProUGUI levelText; // 等级文字 (例如 "等级1")

    // 记录当前槽位显示的技能ID，方便查找
    public int currentSkillID = -1;

    public void InitSlot()
    {
        // 初始化时隐藏或者是默认状态
       

        gameObject.SetActive(false);
    }

    public void UpdateSlot(int level)
    {
        if (!gameObject.activeSelf)
        {


            gameObject.SetActive(true);
        }
        // 更新等级
        if (levelText != null)
        {
            levelText.text = "等级 " + level;
        }
    }
}