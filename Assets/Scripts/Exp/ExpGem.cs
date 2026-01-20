using UnityEngine;

public class ExpGem : MonoBehaviour, IPoolObject
{
    [Header("基础属性")]
    public int expAmount = 10;
    public float moveSpeed = 8f;
    public float pickupDistance = 3f;
    public float freezeTime = 0.5f;

    [Header("地形检测")]
    // 🔥 重要：在 Inspector 里把这个选成 Default 或者 Terrain (不要选 Player 或 Enemy)
    public LayerMask groundLayer;

    private Transform _player;
    private bool _isTargeted = false;
    private float _timer;
    private Vector3 _velocity;

    // 不再使用固定的 FLOOR_Y，而是动态检测
    // private const float FLOOR_Y = 0.15f; 

    public void OnSpawn()
    {
        _isTargeted = false;
        _timer = freezeTime;

        // 模拟一个向上喷出的初速度 (X,Z 随机扩散)
        _velocity = new Vector3(Random.Range(-2f, 2f), 5f, Random.Range(-2f, 2f));

        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 防呆：如果没设置层级，默认检测 Default 层
        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Default", "Terrain", "Ground");
    }

    void Update()
    {
        if (_timer > 0) _timer -= Time.deltaTime;

        // === 阶段 1: 被吸附状态 ===
        if (_isTargeted)
        {
            if (_player == null) return; // 防止玩家死后报错

            // 飞向玩家
            transform.position = Vector3.MoveTowards(transform.position, _player.position, moveSpeed * Time.deltaTime);
            moveSpeed += 20f * Time.deltaTime; // 加速飞行

            // 距离够近就吃掉
            if (Vector3.Distance(transform.position, _player.position) < 0.5f) Collect();
            return;
        }

        // === 阶段 2: 自由落体状态 (核心修改) ===
        // 只有当还有速度（还在运动）的时候才计算物理，落地后就省电不走了
        if (_velocity.sqrMagnitude > 0.01f)
        {
            // 1. 模拟重力 (Y轴持续减速)
            _velocity += Vector3.down * 15f * Time.deltaTime;

            // 2. 预测下一帧的位置
            Vector3 nextPos = transform.position + _velocity * Time.deltaTime;

            // 3. 🔥 射线检测：看看脚下有没有地
            // 从当前位置稍微高一点的地方(防止穿模)向下发射射线
            float checkDistance = Mathf.Abs(_velocity.y * Time.deltaTime) + 0.3f; // 检测距离 = 这一帧走的距离 + 缓冲

            // Raycast(起点, 方向, 结果, 距离, 层级)
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, checkDistance, groundLayer))
            {
                // 🛑 撞到地面了！
                // 把位置修正到地面上方一点点 (0.15f)
                transform.position = hit.point + Vector3.up * 0.15f;

                // 停止运动
                _velocity = Vector3.zero;
            }
            else
            {
                // ✈️ 还没落地，应用位移
                transform.position = nextPos;
            }

            // 保底逻辑：如果地图边缘没有地面，防止掉到无限深渊
            if (transform.position.y < -50f)
            {
                _velocity = Vector3.zero;
                // 或者可以直接 PoolManager.Instance.Despawn(gameObject);
            }
        }

        // === 阶段 3: 触发吸附检测 ===
        if (_timer <= 0 && _player != null)
        {
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < pickupDistance)
            {
                _isTargeted = true;
            }
        }
    }

    void Collect()
    {
        int finalExp = expAmount;
        if (_player != null)
        {
            PlayerController pc = _player.GetComponent<PlayerController>();
            if (pc != null) finalExp = Mathf.RoundToInt(expAmount * pc.expMultiplier);
        }

        if (LevelManager.Instance != null) LevelManager.Instance.AddExp(finalExp);
        PoolManager.Instance.Despawn(gameObject);
    }
}