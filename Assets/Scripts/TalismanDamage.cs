using UnityEngine;

public class TalismanDamage : MonoBehaviour
{
    [Header("战斗参数")]
    public int damageAmount = 10; // 伤害值

    void OnTriggerEnter(Collider other)
    {
        // 只有撞到 Enemy 才生效
        if (other.CompareTag("Enemy"))
        {
            // 1. 尝试获取对方的 Health 组件
            Health enemyHealth = other.GetComponent<Health>();

            if (enemyHealth != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("Hit");
                }
                // 2. 有血条？那就扣血！
                enemyHealth.TakeDamage(damageAmount);
            }
            else
            {
                // 3. 没血条？那还是按照老规矩，直接回收 (防Bug)
                PoolManager.Instance.Despawn(other.gameObject);
            }

            // 这里可以加一个击中特效 (Day 4 内容)
        }
    }
}