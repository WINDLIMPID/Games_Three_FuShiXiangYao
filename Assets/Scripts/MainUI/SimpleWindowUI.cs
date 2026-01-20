using UnityEngine;
using DG.Tweening;

public class SimpleWindowUI : MonoBehaviour
{
    [Header("=== 基础设置 ===")]
    [Tooltip("整个界面的根节点 (包含黑色背景)。开关界面主要控制它。")]
    public GameObject panelRoot;

    [Header("=== 果冻动画设置 ===")]
    [Tooltip("🔥 核心功能：请把你想要弹动的那个子物体(比如中间的窗口)拖到这里！\n如果不拖，就没有动画，直接显示。")]
    public Transform bounceNode;

    [Tooltip("是否启用果冻效果")]
    public bool useBounceAnim = true;

    [Tooltip("动画时长")]
    public float animDuration = 0.4f;

    // 虚方法：子类可以重写
    public virtual void Show()
    {
        if (panelRoot != null)
        {
            // 1. 先把整个界面（含背景）显示出来，这样背景就是瞬间出现的
            panelRoot.SetActive(true);

            // 2. 如果配置了“弹动节点”，就只动那个节点
            if (useBounceAnim && bounceNode != null)
            {
                // 初始化状态：先缩成0 (看不见)
                bounceNode.localScale = Vector3.zero;

                // 播放果冻动画 (OutBack = 回弹)
                bounceNode.DOScale(1f, animDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true); // 忽略 Time.timeScale 的暂停
            }
        }
    }

    public virtual void Hide()
    {
        if (panelRoot != null)
        {
            // 1. 如果有动画节点，先播“缩小”动画，播完再关界面
            if (useBounceAnim && bounceNode != null)
            {
                // InBack = 先夸张放大一点再缩小
                bounceNode.DOScale(0f, animDuration * 0.8f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        // 动画播完了，再把整个背景关掉
                        panelRoot.SetActive(false);
                        OnHideComplete(); // 预留给子类的回调
                    });
            }
            else
            {
                // 没动画，直接关
                panelRoot.SetActive(false);
                OnHideComplete();
            }
        }
    }

    // 这是一个钩子方法，子类如果想在完全关闭后做点什么（比如恢复游戏），可以重写它
    protected virtual void OnHideComplete()
    {
        // 默认什么都不做，子类可以去写 Time.timeScale = 1;
    }
}