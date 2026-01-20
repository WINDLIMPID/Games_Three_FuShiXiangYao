using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
    // 如果只想看 FPS，不想打印 Log 刷屏，可以把这个关掉
    public bool showLog = true;

    void Awake()
    {
        // 1. 关闭垂直同步 (0 = 关闭)
        // 必须关闭 VSync，否则 targetFrameRate 会失效
        QualitySettings.vSyncCount = 0;

        // 2. 🔥 核心修改：锁定帧率
        // 建议：PC/高端机设为 60，低端机设为 30
        Application.targetFrameRate = 60;

        // 如果你的游戏是纯粹的休闲挂机，甚至可以设为 30 以极度省电
        // Application.targetFrameRate = 30; 

        Debug.Log("⚙️ 帧率限制已设置为: " + Application.targetFrameRate);
    }

    // (可选) 这里的 FPS 显示逻辑可以保留，用来在手机上监控是否能稳住 60
    float _frameCount = 0f;
    float _dt = 0f;
    float _fps = 0f;

    void Update()
    {
        if (!showLog) return;

        _frameCount++;
        _dt += Time.deltaTime;

        if (_dt >= 0.5f)
        {
            _fps = _frameCount / _dt;
            _frameCount = 0;
            _dt = 0;

            // 看看现在实际跑多少，如果稳在 59-60 就是完美的
            Debug.Log($"<color=yellow>FPS: {_fps:F1}</color> | <color=red>Active Enemies: {EnemyAI.ActiveCount}</color>");
        }
    }
}