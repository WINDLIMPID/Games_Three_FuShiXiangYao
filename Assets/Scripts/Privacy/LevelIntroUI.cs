using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class LevelIntroUI : MonoBehaviour
{
    [Header("UI 组件")]
    public Image bgOverlay;
    public Image tipImage;
    public CanvasGroup mainCanvasGroup;
    public Sprite normalModeBanner;

    [Header("时间控制")]
    [Range(0.1f, 3f)] public float bgFadeInDuration = 1.0f;
    [Range(0.1f, 3f)] public float bannerPopupDuration = 0.5f;
    [Range(0.5f, 5f)] public float stayDuration = 2.0f;
    [Range(0.1f, 2f)] public float fadeOutDuration = 1.0f;

    void Awake()
    {
        if (mainCanvasGroup != null) mainCanvasGroup.alpha = 0;
        if (bgOverlay) bgOverlay.gameObject.SetActive(false);
        if (tipImage) tipImage.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    // 🔥🔥🔥 1. 自动开始（我是第一棒） 🔥🔥🔥
    IEnumerator Start()
    {
        yield return null;

        // 无尽模式直接跳过介绍，直接开打（或者你也可以让它走一遍流程）
        if (EnemySpawner.Instance != null && EnemySpawner.Instance.IsEndlessMode)
        {
            EnemySpawner.Instance.StartSpawning();
            gameObject.SetActive(false);
            yield break;
        }

        // 设置图片
        if (normalModeBanner != null && tipImage != null)
        {
            tipImage.sprite = normalModeBanner;
            tipImage.SetNativeSize();
        }

        PlayIntroAnimation();
    }

    void PlayIntroAnimation()
    {
        // 确保显示
        gameObject.SetActive(true);
        if (bgOverlay) bgOverlay.gameObject.SetActive(true);
        if (tipImage) tipImage.gameObject.SetActive(true);

        // 初始化状态
        if (bgOverlay) bgOverlay.color = new Color(0, 0, 0, 0);
        if (tipImage) { tipImage.color = new Color(1, 1, 1, 0); tipImage.transform.localScale = Vector3.one * 3f; }
        if (mainCanvasGroup) mainCanvasGroup.alpha = 1;

        Sequence seq = DOTween.Sequence();

        // 动画流程
        if (bgOverlay) seq.Append(bgOverlay.DOColor(new Color(0, 0, 0, 0.85f), bgFadeInDuration).SetEase(Ease.OutSine));

        float bannerInsertTime = bgFadeInDuration * 0.5f;
        if (tipImage)
        {
            seq.Insert(bannerInsertTime, tipImage.DOFade(1, bannerPopupDuration));
            seq.Insert(bannerInsertTime, tipImage.transform.DOScale(1f, bannerPopupDuration).SetEase(Ease.OutBack));
        }

        seq.AppendInterval(stayDuration);

        if (mainCanvasGroup) seq.Append(mainCanvasGroup.DOFade(0, fadeOutDuration));

        // 🔥🔥🔥 2. 介绍播完了，呼叫 TutorialManager（交接棒） 🔥🔥🔥
        seq.OnComplete(() => {

            gameObject.SetActive(false);

            if (TutorialManager.Instance != null)
            {
                // 告诉新手引导：我播完了，该你了。
                // 并给它一个最终任务：等你弄完了，叫 EnemySpawner 刷怪。
                TutorialManager.Instance.CheckAndStartTutorial(() => {
                    if (EnemySpawner.Instance != null)
                    {
                        EnemySpawner.Instance.StartSpawning();
                    }
                });
            }
            else
            {
                // 保底：如果没有引导管理器，直接刷怪
                if (EnemySpawner.Instance != null) EnemySpawner.Instance.StartSpawning();
            }
        });
    }
}