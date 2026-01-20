using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_SkillCard : MonoBehaviour
{
    [Header("组件引用")]
    public Button cardBtn;      // 整个按钮
    public Image bgImage;       // 🔥 这里拖拽按钮上那个大的背景图 Image
                                // public Image iconImage;  // ❌ 这个可以删掉或者留空，因为我们不再需要单独的小图标了

    public TextMeshProUGUI nameText;  // 技能名字
    public TextMeshProUGUI descText;  // 技能描述

    // 如果你还需要显示 "境界一"，保留这个；如果像截图一样不显示，可以不管它
    public TextMeshProUGUI levelText;

    private void Awake()
    {
        if (cardBtn == null) cardBtn = GetComponent<Button>();
        // 如果没拖拽 bgImage，尝试自动获取按钮自己的 Image
        if (bgImage == null) bgImage = GetComponent<Image>();
    }

    // 初始化方法
    public void Setup(Sprite cardSprite, string name, string levelStr, string desc, Action onClick)
    {
        // 🔥 核心修改：把传入的 icon (cardSprite) 直接赋给背景图
        if (bgImage != null && cardSprite != null)
        {
            bgImage.sprite = cardSprite;
            bgImage.enabled = true; // 确保图片是显示的
        }

        // 设置文字
        if (nameText != null) nameText.text = name;
        if (descText != null) descText.text = desc;

        // 处理等级文字 (如果你想隐藏它，就把 levelText 设为 null 或者 SetActive(false))
        if (levelText != null)
        {
            // 如果你想完全隐藏等级，取消下面这行的注释：
            // levelText.gameObject.SetActive(false);

            // 或者继续显示：
            levelText.text = levelStr;
        }

        // 绑定点击事件
        if (cardBtn != null)
        {
            cardBtn.onClick.RemoveAllListeners();
            cardBtn.onClick.AddListener(() => {
                onClick?.Invoke();
            });
        }
    }
}