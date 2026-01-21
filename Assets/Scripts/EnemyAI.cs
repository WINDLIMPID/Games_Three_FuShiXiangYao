using UnityEngine;
using System.Collections;

// 确保状态定义完整
public enum EnemyState { Wander, Chase, Attack, Hit, Die }

public class EnemyAI : MonoBehaviour, IPoolObject
{
    public static int ActiveCount = 0;

    [Header("状态监控")]
    public EnemyState currentState = EnemyState.Wander;
    private bool _isDeadLock = false; // 死亡锁，一旦为true，禁止任何其他逻辑

    [Header("距离配置")]
    public float detectRange = 10.0f;
    public float attackRange = 2.2f;    // 稍微调大一点，匹配动画挥动距离
    public float stopThreshold = 3.0f;  // 脱离距离

    [Header("战斗配置")]
    public float attackCooldown = 1.5f;
    private float _lastAttackTime;

    [Header("移动配置")]
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 4.5f;

    [Header("引用")]
    public Animator animator;
    public GameObject expGemPrefab;
    private Transform _player;
    private Rigidbody _rb;
    private Health _health;

    public void OnSpawn()
    {
        this.enabled = true;

        _isDeadLock = false;
        currentState = EnemyState.Wander;
        _lastAttackTime = 0;

        // 1. 恢复物理碰撞
        var col = GetComponent<Collider>();
        if (col) col.enabled = true;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // 2. 动画重置
        if (animator)
        {
            animator.Rebind();
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Die");
        }

        // 3. 🔥 强制查找并开启子物体中的血条
        // 假设你的血条挂在名为 "HealthBar" 或带有 HealthBar 脚本的子物体上
        HealthBar hb = GetComponentInChildren<HealthBar>(true); // true 表示包含隐藏的子物体
        if (hb != null)
        {
            hb.gameObject.SetActive(true);
        }

        // 🔥 新增：出生特效（瞬间变小再变大）
        transform.localScale = Vector3.zero;
        StartCoroutine(SpawnEffect());
    }
    System.Collections.IEnumerator SpawnEffect()
    {
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            // 0.5秒内从 0 变大到 1
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / 0.5f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
    void OnEnable()
    {
        ActiveCount++;
        _rb = GetComponent<Rigidbody>();
        _health = GetComponent<Health>();

        if (_health != null)
        {
            _health.OnDeath += HandleDeath;
            _health.OnDamageTaken += HandleHit;
            _health.OnSpawn();
        }

        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 🔥 新增：监听玩家的死亡事件
        if (_player != null)
        {
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += OnPlayerDead;
            }
        }
    }

    void OnDisable()
    {
        ActiveCount--;
        if (_health != null)
        {
            _health.OnDeath -= HandleDeath;
            _health.OnDamageTaken -= HandleHit;
        }

        // 🔥 记得取消监听
        if (_player != null)
        {
            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null) playerHealth.OnDeath -= OnPlayerDead;
        }
    }

    // 🔥 玩家死后，敌人进入待机或巡逻状态
    void OnPlayerDead()
    {
        currentState = EnemyState.Wander;
        // 也可以让动画变回 Idle
        if (animator) animator.SetFloat("Speed", 0f);
    }

    void Update()
    {
        // 🔥 核心保护 1：死亡锁开启后，彻底切断 Update 逻辑
        if (_isDeadLock || _player == null) return;

        // 🔥 新增：检查玩家是否已经死亡
        Health playerHealth = _player.GetComponent<Health>();
        if (playerHealth != null && playerHealth.isDead)
        {
            // 玩家死了，敌人回到徘徊状态或者待机
            currentState = EnemyState.Wander;
            if (animator) animator.SetFloat("Speed", 0f);
            return;
        }

        float dist = Vector3.Distance(transform.position, _player.position);

        // 状态切换逻辑
        if (currentState != EnemyState.Hit)
        {
            if (dist <= attackRange)
            {
                currentState = EnemyState.Attack;
                HandleAttackLogic();
            }
            else if (dist > stopThreshold)
            {
                if (dist <= detectRange) currentState = EnemyState.Chase;
                else currentState = EnemyState.Wander;
            }
        }

        // 动画控制
        if (animator)
        {
            float targetSpeed = (currentState == EnemyState.Wander) ? 1.0f : (currentState == EnemyState.Chase ? 2.0f : 0f);
            animator.SetFloat("Speed", targetSpeed, 0.15f, Time.deltaTime);
        }
    }

    void HandleAttackLogic()
    {
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            if (animator)
            {
                animator.SetInteger("AttackNum", Random.Range(0, 3));
                animator.SetTrigger("Attack");

                //攻击触发伤害
                ApplyDamageToPlayer();
            }
            _lastAttackTime = Time.time;
        }
    }

    public void ApplyDamageToPlayer()
    {
        // 只有在没死且玩家存在的情况下才执行
        if (_isDeadLock || _player == null) return;

        Health playerHealth = _player.GetComponent<Health>();
        // 如果玩家已经死了，敌人直接进入空闲/徘徊状态
        if (playerHealth != null && playerHealth.isDead)
        {
            currentState = EnemyState.Wander;
            return;
        }

        float dist = Vector3.Distance(transform.position, _player.position);

        // 如果玩家在攻击范围内
        if (dist <= attackRange)
        {
            if (playerHealth != null)
            {
                // 伤害数值可以根据敌人类型设置，这里暂定 10
                playerHealth.TakeDamage(10);
                Debug.Log("玩家受到攻击！");
            }
        }
    }
    void FixedUpdate()
    {
        // 🔥 核心保护 2：死亡锁开启后，物理速度清零并禁止位移
        if (_isDeadLock || _player == null)
        {
            if (_rb) _rb.velocity = Vector3.zero;
            return;
        }

        if (currentState == EnemyState.Wander) MoveTo(wanderSpeed);
        else if (currentState == EnemyState.Chase) MoveTo(chaseSpeed);
        else
        {
            _rb.velocity = Vector3.zero;
            // 攻击时保持面向玩家
            Vector3 lookDir = _player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero) transform.forward = lookDir;
        }
    }

    void MoveTo(float speed)
    {
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
        _rb.MovePosition(transform.position + dir * speed * Time.fixedDeltaTime);
    }

    public void HandleHit(int d)
    {
        // 🔥 核心保护 3：受击时判断死亡锁，防止尸体被打得动来动去
        if (_isDeadLock) return;

        currentState = EnemyState.Hit;
        if (animator)
        {
            animator.SetTrigger("Hit");
        }

        CancelInvoke("RecoverState");
        Invoke("RecoverState", 0.35f);
    }

    void RecoverState()
    {
        // 恢复前再次检查死亡锁
        if (!_isDeadLock) currentState = EnemyState.Chase;
    }

    void HandleDeath()
    {
        if (_isDeadLock) return; // 防止重复触发死亡
        _isDeadLock = true;
        currentState = EnemyState.Die;

        // 立即关闭碰撞，子弹就再也打不中了
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (animator)
        {
            animator.SetTrigger("Die");
        }
        // 🔥 1. 加分
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.AddScore(1);
        }
        // 掉落经验球
        if (expGemPrefab)
        {
            PoolManager.Instance.Spawn(expGemPrefab, transform.position + Vector3.up, Quaternion.identity);
        }

        //Invoke("DespawnSelf", 2.0f);
    }

    //void DespawnSelf() => PoolManager.Instance.Despawn(this.gameObject);
}