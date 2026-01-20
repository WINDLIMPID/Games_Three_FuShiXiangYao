using UnityEngine;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [System.Serializable]
    public class VFXData
    {
        public string name;         // 特效名字 (例如 "Hit", "Die")
        public GameObject prefab;   // 特效预制体
        public int poolSize = 10;   // 初始池子大小
    }

    [Header("特效配置")]
    public List<VFXData> vfxLibrary;

    // 运行时对象池字典: 名字 -> 队列
    private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    // 查找字典: 名字 -> 预制体信息
    private Dictionary<string, VFXData> _lookup = new Dictionary<string, VFXData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializePools();
    }

    void InitializePools()
    {
        // 创建父节点，防止Hierarchy太乱
        GameObject poolRoot = new GameObject("VFX_Pool_Root");
        poolRoot.transform.SetParent(transform);

        foreach (var data in vfxLibrary)
        {
            if (string.IsNullOrEmpty(data.name) || data.prefab == null) continue;

            _lookup[data.name] = data;
            _pools[data.name] = new Queue<GameObject>();

            // 预先生成一堆特效，关掉备用
            for (int i = 0; i < data.poolSize; i++)
            {
                CreateNewInstance(data.name, data.prefab, poolRoot.transform);
            }
        }
    }

    GameObject CreateNewInstance(string name, GameObject prefab, Transform parent)
    {
        GameObject obj = Instantiate(prefab, parent);
        obj.SetActive(false);
        _pools[name].Enqueue(obj);
        return obj;
    }

    // ============================================
    // 🔥 核心功能：播放特效
    // ============================================
    public void PlayVFX(string name, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(name))
        {
            Debug.LogWarning($"⚠️ 找不到特效: {name}");
            return;
        }

        // 1. 从池子里取
        Queue<GameObject> queue = _pools[name];
        GameObject vfxObj;

        if (queue.Count == 0)
        {
            // 池子空了，临时加一个 (扩容)
            if (_lookup.TryGetValue(name, out VFXData data))
            {
                vfxObj = Instantiate(data.prefab, transform.GetChild(0)); // 放在PoolRoot下
            }
            else return;
        }
        else
        {
            vfxObj = queue.Dequeue();
        }

        // 2. 设置位置并激活
        vfxObj.transform.position = position;
        vfxObj.transform.rotation = rotation;
        vfxObj.SetActive(true);

        // 3. 播放粒子系统 (如果根节点是 ParticleSystem)
        ParticleSystem ps = vfxObj.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();

        // 4. 自动回收 (根据粒子时长，或者固定时间)
        // 注意：我们这里用一个协程或者简单的方法来回收
        // 为了性能，建议粒子系统勾选 "Stop Action -> Disable" (Unity自带功能)，
        // 但为了把物体放回 Queue，我们需要手动处理。
        StartCoroutine(RecycleVFX(name, vfxObj, 2.0f)); // 假设所有特效最长2秒
    }

    System.Collections.IEnumerator RecycleVFX(string name, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        _pools[name].Enqueue(obj);
    }
}