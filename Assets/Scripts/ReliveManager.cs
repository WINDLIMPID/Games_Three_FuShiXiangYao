using UnityEngine;
using UnityEngine.UI;

public class ReliveManager : MonoBehaviour
{
    public static ReliveManager Instance;

    [Header("UI 引用")]
    public GameObject relivePanel;    // 拖入你的 "ReLive" 面板对象
    public GameObject failPanel;      // 拖入你的 "GameEndUI" (真正的失败结算面板)

    [Header("按钮引用")]
    public Button adReviveBtn;        // "观看仙缘..." 按钮
    public Button giveUpBtn;          // "放弃挑战" 按钮

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 绑定按钮事件
        if (adReviveBtn != null)
            adReviveBtn.onClick.AddListener(OnAdReviveClicked);

        if (giveUpBtn != null)
            giveUpBtn.onClick.AddListener(OnGiveUpClicked);

        // 确保一开始是隐藏的
        if (relivePanel != null) relivePanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
    }

    // 🔥 1. 外部调用的入口：显示复活界面
    public void ShowRelivePanel()
    {
        if (relivePanel != null)
        {
            relivePanel.SetActive(true);

            // 暂停游戏，防止怪物继续攻击尸体
            Time.timeScale = 0f;
        }
    }

    // 🔥 2. 点击 "观看广告复活"
    void OnAdReviveClicked()
    {
        // TODO: 这里接入你的广告 SDK (比如 AdMob / 穿山甲)
        // 目前我们模拟广告播放成功
        Debug.Log("📺 广告播放成功，准备复活玩家...");

        RevivePlayer();
    }

    // 执行复活逻辑
    void RevivePlayer()
    {
        // 1. 恢复时间
        Time.timeScale = 1f;

        // 2. 关闭复活界面
        if (relivePanel != null) relivePanel.SetActive(false);
        // 👇 这一句必须加！否则你人活了，屏幕还是被挡住的
        if (failPanel != null) failPanel.SetActive(false);
        // 3. 找到玩家并复活
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null)
            {
                hp.Resurrect(); // 🔥 调用我们将在 Health 里新写的方法
            }
        }
    }

    // 🔥 3. 点击 "放弃挑战"
    void OnGiveUpClicked()
    {
        // 恢复时间 (或者保持暂停，看你失败界面的需求)
        // 通常失败结算时游戏也是暂停的，这里我们先保持暂停
        // Time.timeScale = 1f; 

        // 关闭复活界面
        if (relivePanel != null) relivePanel.SetActive(false);

     
        // 2. 🔥 核心修改：判断是“无尽模式”还是“普通模式”
        if (EnemySpawner.Instance != null && EnemySpawner.Instance.IsEndlessMode)
        {
            // 如果是无尽模式 -> 呼叫无尽结算面板
            EnemySpawner.Instance.OnEndlessModeGameOver();
        }
        else
        {
            // 打开真正的失败界面
            if (failPanel != null) failPanel.SetActive(true);
        }
        Debug.Log("💀 玩家放弃复活，进入结算...");
    }
}