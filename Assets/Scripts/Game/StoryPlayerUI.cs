using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class StoryPage
{
    public Sprite image;      // 这一页的漫画图
    public AudioClip audio;   // 这一页的配音/BGM
}

public class StoryPlayerUI : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject panelRoot;      // 界面根节点
    public Image displayImage;        // 用于显示漫画的 Image
    public Button nextStepBtn;        // 全屏透明按钮（点击任意处下一页）
    public Button finishBtn;          // 最后一页显示的“开始/关闭”按钮
    public AudioSource audioSource;   // 用于播放配音

    [Header("内容配置")]
    public List<StoryPage> pages;     // 在 Inspector 里拖入你的4张图和音频

    // 内部状态
    private int _currentIndex = 0;
    private UnityAction _onCompleteCallback; // 播放结束后的回调

    void Start()
    {
        // 绑定按钮事件
        if (nextStepBtn) nextStepBtn.onClick.AddListener(OnNextClick);
        if (finishBtn) finishBtn.onClick.AddListener(OnFinishClick);

        // 初始隐藏
        // panelRoot.SetActive(false); 
    }

    /// <summary>
    /// 开始播放漫画
    /// </summary>
    /// <param name="onComplete">播放结束要做什么（进入游戏 or 关闭弹窗）</param>
    public void PlayStory(UnityAction onComplete)
    {
        _onCompleteCallback = onComplete;
        _currentIndex = 0;

        panelRoot.SetActive(true);
        finishBtn.gameObject.SetActive(false); // 隐藏最终按钮
        nextStepBtn.gameObject.SetActive(true); // 启用点击下一步

        ShowPage(_currentIndex);
    }

    void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count) return;

        StoryPage page = pages[index];

        // 1. 换图
        if (displayImage && page.image)
        {
            displayImage.sprite = page.image;
        }

        // 2. 播音
        if (audioSource && page.audio)
        {
            audioSource.Stop();
            audioSource.clip = page.audio;
            audioSource.Play();
        }

        // 3. 如果是最后一页，显示最终按钮
        if (index == pages.Count - 1)
        {
            finishBtn.gameObject.SetActive(true);
            nextStepBtn.gameObject.SetActive(false); // 最后一页禁止点击屏幕切换，强制点按钮
        }
    }

    void OnNextClick()
    {
        // 还没到最后一页，继续翻页
        if (_currentIndex < pages.Count - 1)
        {
            _currentIndex++;
            ShowPage(_currentIndex);
        }
    }

    void OnFinishClick()
    {
        // 停止音频
        if (audioSource) audioSource.Stop();

        // 关闭界面
        panelRoot.SetActive(false);

        // 执行回调（比如进入游戏，或者仅仅是关闭）
        _onCompleteCallback?.Invoke();
    }
}