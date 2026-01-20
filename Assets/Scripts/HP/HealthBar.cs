using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI 组件")]
    public Image fillImage;   // 前景血条（红色/绿色）
    public Image delayImage;  // 背景残影（白色/黄色）

    [Header("数据引用")]
    public Health health;     // 绑定的血量组件

    [Header("平滑设置")]
    public float smoothSpeed = 5f; // 残影追赶速度
    public bool alwaysFaceCamera = true; // 🔥 开关：是否始终看向相机

    private float _targetFillAmount; // 目标血量比例
    private Camera _mainCamera;
    private Canvas _canvas;

    void Awake()
    {
        // 自动查找 Health 组件 (如果在父物体上)
        if (health == null)
        {
            health = GetComponentInParent<Health>();
        }

        // 缓存相机引用，性能更好
        _mainCamera = Camera.main;
        _canvas = GetComponent<Canvas>();
    }

    void OnEnable()
    {
        // 1. 绑定事件
        if (health != null)
        {
            health.OnHealthChanged += OnHealthChangedHandle;

            // 🔥 关键修复：启用时立刻刷新一次，防止血条为空
            UpdateBar(health.currentHealth, health.maxHealth, true);
        }
    }

    void OnDisable()
    {
        // 2. 解绑事件 (防止内存泄漏)
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChangedHandle;
        }
    }

    void OnHealthChangedHandle(int current, int max)
    {
        UpdateBar(current, max);

        // 如果你有自动隐藏逻辑，可以写在这里
        // 比如满血时隐藏:
        // if (_canvas != null) _canvas.enabled = (current < max);
    }

    // 更新血条的核心逻辑
    void UpdateBar(int current, int max, bool instant = false)
    {
        if (max <= 0) return;

        _targetFillAmount = (float)current / max;

        // 如果是初始化(instant)，直接瞬间设置，不要动画
        if (instant)
        {
            if (fillImage != null) fillImage.fillAmount = _targetFillAmount;
            if (delayImage != null) delayImage.fillAmount = _targetFillAmount;
        }
    }

    void LateUpdate()
    {
        // 🔥 修复问题2：让血条始终看向相机
        // 使用 LateUpdate 确保在相机移动后才旋转 UI，防止抖动
        if (alwaysFaceCamera && _mainCamera != null)
        {
            // 方法A：简单看向 (有时候会镜像翻转，看你相机的设置)
            // transform.LookAt(transform.position + _mainCamera.transform.forward);

            // 方法B：直接对齐旋转 (最稳定，推荐)
            transform.rotation = _mainCamera.transform.rotation;
        }
    }

    void Update()
    {
        // 1. 处理主血条 (稍微带点平滑，视觉更好)
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, _targetFillAmount, Time.deltaTime * 20f);
        }

        // 2. 处理残影血条 (慢动作追赶)
        if (delayImage != null)
        {
            if (delayImage.fillAmount > fillImage.fillAmount)
            {
                // 只有扣血时才慢动作追赶 (打击感来源)
                delayImage.fillAmount = Mathf.Lerp(delayImage.fillAmount, fillImage.fillAmount, Time.deltaTime * smoothSpeed);
            }
            else
            {
                // 回血时瞬间跟上，不要让残影比血条还少
                delayImage.fillAmount = fillImage.fillAmount;
            }
        }
    }
}