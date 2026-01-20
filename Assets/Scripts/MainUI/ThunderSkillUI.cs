using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThunderSkillUI : MonoBehaviour
{
    [Header("UI 设置")]
    public Button useButton;          // 拖入你的雷符图标按钮
    public TextMeshProUGUI countText; // 拖入显示数量的文字

    [Header("战斗参数")]
    public int damageAmount = 99999; // 秒杀伤害，设个大数

    private void Start()
    {
        // 绑定点击事件
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnUseThunderClicked);
        }

        // 刚进入战斗时刷新一下数量
        RefreshUI();
    }

    private void OnUseThunderClicked()
    {
        if (ItemManager.Instance == null) return;

        // 1. 尝试消耗 1 张符
        bool isSuccess = ItemManager.Instance.UseThunder(1);

        if (isSuccess)
        {
            // 2. 执行清屏！
            KillAllEnemies();

            // 3. 刷新数量显示
            RefreshUI();
        }
        else
        {
            Debug.Log("雷神符不足！");
        }
    }

    // 🔥🔥🔥 核心：一键清屏逻辑 🔥🔥🔥
    private void KillAllEnemies()
    {
        // 1. 播放全屏特效 (可选)
        if (VFXManager.Instance != null)
        {
            // ⚠️ 记得在 VFXManager 里配一个叫 "ThunderScreen" 的特效
            VFXManager.Instance.PlayVFX("ThunderScreen", transform.position, Quaternion.identity);
        }

        // 2. 播放音效 (可选)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("LevelUp"); // 暂时借用升级音效，听起来比较响
        }

        // 3. 找到所有标签为 "Enemy" 的物体
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length > 0)
        {
            Debug.Log($"⚡ 雷神降世！瞬间消灭 {enemies.Length} 个敌人！");
        }

        foreach (var enemyObj in enemies)
        {
            // 获取怪物的血条组件
            Health enemyHealth = enemyObj.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.isDead)
            {
                // 造成巨额伤害，触发死亡逻辑 (这样会有死亡动画、掉落经验球)
                enemyHealth.TakeDamage(damageAmount);
            }
            else
            {
                // 如果怪物没有血条脚本，直接销毁 (保底)
                if (PoolManager.Instance != null) PoolManager.Instance.Despawn(enemyObj);
                else Destroy(enemyObj);
            }
        }
    }

    private void RefreshUI()
    {
        if (ItemManager.Instance != null && countText != null)
        {
            int count = ItemManager.Instance.GetThunderCount();
            countText.text = "x" + count;

            // 没符了就把按钮变灰，防止误触
            if (useButton != null) useButton.interactable = (count > 0);
        }
    }
}