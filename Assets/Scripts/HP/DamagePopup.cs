using UnityEngine;
using TMPro; // ★ 必须引入这个命名空间

// 实现 IPoolObject 接口，方便对象池管理
public class DamagePopup : MonoBehaviour, IPoolObject
{
    private TextMeshPro _textMesh;
    private float _disappearTimer; // 消失倒计时
    private Color _textColor;      // 用来控制透明度
    private Vector3 _moveVector;   // 移动方向

    private const float DISAPPEAR_TIMER_MAX = 0.8f; // 存在时间(秒)

    void Awake()
    {
        // 获取自身的 TMP 组件
        _textMesh = GetComponentInChildren<TextMeshPro>();
    }

    // ★ 核心方法：外部调用它来设置数字
    public void Setup(int damageAmount)
    {
        _textMesh.text = damageAmount.ToString();

        // 重置状态
        _textColor = _textMesh.color;
        _textColor.a = 1f; // 完全不透明
        _textMesh.color = _textColor;
        _disappearTimer = DISAPPEAR_TIMER_MAX;

        // 设置一个向上的初速度，并带一点随机左右偏移，让画面更生动
        _moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 2f, 0) * 3f;

        // 出生时大一点，然后迅速缩小，制造“弹出感”
        _textMesh.transform.localScale = Vector3.one * 0.15f;
    }

    // IPoolObject 接口实现
    public void OnSpawn()
    {
        // 逻辑都在 Setup 里手动调，这里留空即可
    }

    void Update()
    {
        // 1. 向上移动
        transform.position += _moveVector * Time.deltaTime;
        // 模拟空气阻力，让上升速度越来越慢
        _moveVector -= _moveVector * 5f * Time.deltaTime;

        // 2. 缩放回弹效果 (从 0.15 缩回到正常大小 0.1)
        if (_textMesh.transform.localScale.x > 0.1f)
        {
            _textMesh.transform.localScale -= Vector3.one * 0.5f * Time.deltaTime;
        }

        // 3. 淡出逻辑
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer < 0)
        {
            // 倒计时结束，开始变透明
            float disappearSpeed = 3f;
            _textColor.a -= disappearSpeed * Time.deltaTime;
            _textMesh.color = _textColor;

            // 完全透明后，回收进对象池
            if (_textColor.a <= 0)
            {
                PoolManager.Instance.Despawn(gameObject);
            }
        }
    }

    // ★ 重要：让文字始终正对摄像机 (Billboard效果)
    // 否则玩家转视角时看不清数字
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // 让自己旋转到和摄像机一模一样的角度
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}