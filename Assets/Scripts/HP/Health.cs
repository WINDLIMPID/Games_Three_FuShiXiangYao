using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("属性")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("状态")]
    public bool isDead = false;
    public bool isInvincible = false;

    [Header("UI 引用")]
    public Slider healthSlider;

    public event Action<int> OnDamageTaken;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    void Start()
    {
        // 确保一开始血量是对的
        currentHealth = maxHealth;
        UpdateUI();
    }

    // 对象池生成或复活时通用
    public void OnSpawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        isInvincible = false;

        this.enabled = true;
        ToggleComponents(true);

        UpdateUI();
    }
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // 🛡️ 无敌判定
        if (isInvincible)
        {
            // 这里加上名字，你就知道是 "Archer" (玩家) 触发了无敌
            Debug.Log($"🛡️ [{gameObject.name}] 处于无敌状态，免疫本次伤害！");
            return;
        }
        // 🔥🔥🔥 新增：播放受伤音效 (通用) 🔥🔥🔥
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Hit");
        }

        // 🔥 2. 播放受击特效 (在受击位置，或者胸口位置)
        if (VFXManager.Instance != null)
        {
            // transform.position + Vector3.up * 1f 意味着在脚底往上1米(胸口)的位置播放
            VFXManager.Instance.PlayVFX("Hit", transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 🔥🔥🔥 核心修改：加上 [gameObject.name] 🔥🔥🔥
        // 这样日志就会变成： "[Archer] 受到伤害: 10..." 或者 "[Enemy_01] 受到伤害: 10..."
        //Debug.Log($"💥 [{gameObject.name}] 受到伤害: {damage} | 剩余血量: {currentHealth}/{maxHealth}");

        UpdateUI();
        OnDamageTaken?.Invoke(damage);

        if (currentHealth <= 0)
        {
            //Debug.Log($"💀 [{gameObject.name}] 血量归零，触发死亡！");
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateUI();
        //Debug.Log($"❤️ 获得治疗: {amount} | 当前血量: {currentHealth}/{maxHealth}");
    }

    public void UpdateUI()
    {
        if (healthSlider != null)
        {
            // 🔥 关键修复：时刻确保血条上限是正确的
            if (healthSlider.maxValue != maxHealth)
            {
                healthSlider.maxValue = maxHealth;
            }
            healthSlider.value = currentHealth;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Resurrect()
    {
        // 1. 数据重置
        isDead = false;

        // ⚠️ 注意：如果你之前的升级增加了 maxHealth，这里应该保持那个数值
        // 如果这里莫名变回 100 了，说明你的 maxHealth 数据丢失了
        if (maxHealth <= 0) maxHealth = 100; // 保底

        currentHealth = maxHealth;
        isInvincible = false;

        // 2. 强制刷新 UI
        UpdateUI();
        Debug.Log($"✨ 复活成功！当前血量: {currentHealth}/{maxHealth} (若此数值过低，请检查升级数据是否丢失)");

        // 3. 启用组件
        this.enabled = true;
        ToggleComponents(true);

        // 4. 重置动画
        if (gameObject.CompareTag("Player"))
        {
            Animator anim = GetComponent<Animator>();
            if (anim == null) anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.Rebind();
                anim.Play("Idle");
            }

            // 5. 开启3秒无敌
            StopCoroutine("InvincibilityRoutine");
            StartCoroutine("InvincibilityRoutine", 3.0f);
        }
    }

    IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log($"🛡️ 玩家进入无敌模式 ({duration}秒)...");

        // 这里可以加一个简单的闪烁效果提示玩家

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        Debug.Log("🛡️ 无敌模式结束，开始承受伤害");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();

        Debug.Log($"💀 {gameObject.name} 已死亡...");

        if (gameObject.CompareTag("Enemy"))
        {
            // 🔥 3. 播放死亡烟雾特效
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayVFX("Die", transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            Destroy(gameObject); // 销毁物体
        }
        Animator anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Die");

        ToggleComponents(false); // 禁用碰撞和控制

        if (gameObject.CompareTag("Player"))
        {
            if (ReliveManager.Instance != null)
            {
                ReliveManager.Instance.ShowRelivePanel();
            }
        }
        else
        {
            Destroy(gameObject, 1.5f);
        }
    }

    // 辅助方法：统一开关组件
    void ToggleComponents(bool state)
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = state;

        if (gameObject.CompareTag("Player"))
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = state;
        }
        else
        {
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = state;
        }
    }
}