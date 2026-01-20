using UnityEngine;

public class TalismanOrbit : MonoBehaviour
{
    [Header("属性配置")]
    public GameObject talismanPrefab;

    // 🔥 新增：当前伤害值 (默认为10)
    public int currentDamage = 10;

    public int count = 1;             // 初始默认送1个
    public float radius = 2.0f;       // 初始范围
    public float rotateSpeed = 180f;   // 初始速度
    public float heightOffset = 1.2f;

    private Transform _container;
    private int _lastCount = -1;

    void Start()
    {
        // 从全局配置读取初始值
        if (GlobalConfig.Instance != null)
        {
            count = GlobalConfig.Instance.initialTalismanCount;
            rotateSpeed = GlobalConfig.Instance.initialRotateSpeed;
            radius = GlobalConfig.Instance.initialRadius;
        }

        GameObject go = new GameObject("TalismanPivot");
        _container = go.transform;

        RebuildFormation();
    }

    void LateUpdate()
    {
        if (_container == null) return;

        // 容器跟随玩家位置
        _container.position = transform.position + Vector3.up * heightOffset;

        // 旋转
        _container.Rotate(Vector3.up, rotateSpeed * Time.unscaledDeltaTime, Space.Self);

        // 检测数量变化
        if (count != _lastCount)
        {
            RebuildFormation();
            _lastCount = count;
        }
    }

    // 🔥🔥🔥 新增：供 UpgradeManager 调用的加攻方法 🔥🔥🔥
    public void AddDamage(int amount)
    {
        currentDamage += amount;
        Debug.Log($"⚔️ 伤害提升！当前伤害: {currentDamage}");

        // 立即更新当前所有正在转圈的符文
        if (_container != null)
        {
            foreach (Transform child in _container)
            {
                var dmgScript = child.GetComponent<TalismanDamage>();
                if (dmgScript != null)
                {
                    dmgScript.damageAmount = currentDamage;
                }
            }
        }
    }

    public void RebuildFormation()
    {
        // 清理旧符文
        for (int i = _container.childCount - 1; i >= 0; i--)
        {
            Destroy(_container.GetChild(i).gameObject);
        }

        if (count <= 0) return;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            float radian = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(Mathf.Cos(radian) * radius, 0, Mathf.Sin(radian) * radius);

            GameObject t = Instantiate(talismanPrefab, _container);
            t.transform.localPosition = pos;
            t.transform.localRotation = Quaternion.Euler(90, -angle, 0);

            if (t.GetComponent<Rigidbody>()) t.GetComponent<Rigidbody>().isKinematic = true;

            // 🔥🔥🔥 关键修改：生成时，把当前的伤害值赋给符文 🔥🔥🔥
            TalismanDamage dmgScript = t.GetComponent<TalismanDamage>();
            if (dmgScript != null)
            {
                dmgScript.damageAmount = currentDamage;
            }
            else
            {
                // 防呆：万一 prefab 上没挂脚本
                Debug.LogWarning("⚠️ 符文 Prefab 上缺少 TalismanDamage 脚本！伤害无法生效。");
            }
        }
    }
}