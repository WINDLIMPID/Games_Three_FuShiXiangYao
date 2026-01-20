using UnityEngine;

public class EnemyVFX : MonoBehaviour
{
    [Header("配置")]
    public GameObject damagePopupPrefab; // 拖入刚才做的数字预制体
    [SerializeField] private float _popupOffset = 1.5f; // 向头顶偏移的高度

    private Health _health;

    void Awake()
    {
        _health = GetComponent<Health>();
    }

    void OnEnable()
    {
        // 监听 Health 脚本的受伤事件
        if (_health != null)
        {
            _health.OnDamageTaken += SpawnDamagePopup;
        }
    }

    void OnDisable()
    {
        // 记得取消监听，防止报错
        if (_health != null)
        {
            _health.OnDamageTaken -= SpawnDamagePopup;
        }
    }

    // 当收到受伤事件时触发
    void SpawnDamagePopup(int damageAmount)
    {
        // 1. 计算生成位置 (怪物头顶)
        Vector3 spawnPos = transform.position + Vector3.up * _popupOffset;

        // 2. 从对象池生成数字
        GameObject popupObj = PoolManager.Instance.Spawn(damagePopupPrefab, spawnPos, Quaternion.identity);

        // 3. 获取脚本并设置具体的数字
        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            popupScript.Setup(damageAmount);
        }
    }
}