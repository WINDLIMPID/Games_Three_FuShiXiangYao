using UnityEngine;
using UnityEngine.UI;

public class HandbookUI : SimpleWindowUI
{
    [Header("=== 核心控制 ===")]
    public GameObject skillPageRoot; // 法术页面父物体 (ScrollView)
    public GameObject enemyPageRoot; // 怪物页面父物体 (ScrollView)

    [Header("=== 标签按钮 ===")]
    public Button skillTabBtn;       // 法术标签按钮
    public Button enemyTabBtn;       // 怪物标签按钮

    [Header("=== 按钮颜色控制 ===")]
    // 选中时的颜色（比如深灰：150, 150, 150）
    public Color selectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    // 未选中的颜色（通常是纯白：255, 255, 255，显示原图）
    public Color normalColor = Color.white;

    [Header("=== 关闭控制 ===")]
    public Button closeBtn;          // 右上角叉叉
    public Button blackMaskBtn;      // 背景全屏黑底按钮

    void Start()
    {
        // 绑定点击事件
        // true = 法术页, false = 怪物页
        skillTabBtn.onClick.AddListener(() => SelectTab(true));
        enemyTabBtn.onClick.AddListener(() => SelectTab(false));

        // 绑定关闭事件
        if (closeBtn) closeBtn.onClick.AddListener(Hide);
        if (blackMaskBtn) blackMaskBtn.onClick.AddListener(Hide);
    }

    // 🔥 这里就是实现“默认进入第一个”的地方
    public override void Show()
    {
        base.Show();     // 播放弹窗动画
        SelectTab(true); // 强制选中法术页 (true)
    }

    // 切换逻辑
    private void SelectTab(bool isSkillTab)
    {
        // 1. 页面显隐：是法术就显示法术页，否则显示怪物页
        if (skillPageRoot) skillPageRoot.SetActive(isSkillTab);
        if (enemyPageRoot) enemyPageRoot.SetActive(!isSkillTab);

        // 2. 按钮变色：选中的变深色，没选中的变回原色
        UpdateButtonColor(skillTabBtn, isSkillTab);   // 如果是法术页，法术按钮变深
        UpdateButtonColor(enemyTabBtn, !isSkillTab);  // 如果是怪物页，怪物按钮变深
    }

    // 变色辅助方法
    private void UpdateButtonColor(Button btn, bool isSelected)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = isSelected ? selectedColor : normalColor;
        }
    }
}