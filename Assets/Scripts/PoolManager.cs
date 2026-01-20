using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 单例模式：保证全局只有一个池子管理器
    public static PoolManager Instance;

    // 核心容器：字典 <预制体名字, 对象队列>
    // 使用 Queue 而不是 List，因为我们只需要"拿一个"和"还一个"，不需要遍历，Queue性能更好
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // 也就是父物体容器，让Hierarchy看起来整洁，不至于满屏都是克隆体
    private Transform poolParent;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 创建一个总的父物体
        poolParent = new GameObject("--- Object Pool ---").transform;
    }

    /// <summary>
    /// 从池子里取对象（如果没有就生成新的）
    /// </summary>
    /// <param name="prefab">想要生成的预制体</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <returns>取出的物体</returns>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 1. 确保 Key 存在
        string key = prefab.name;
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary.Add(key, new Queue<GameObject>());
        }

        GameObject objToSpawn;

        // 2. 尝试从队列里取出一个闲置的
        // Queue.Count > 0 且取出的物体不为空（防止被意外Destroy）
        if (poolDictionary[key].Count > 0)
        {
            objToSpawn = poolDictionary[key].Dequeue();

            // 安全检查：万一这个物体在池子里被外部强制Destroy了，就递归再拿下一个
            if (objToSpawn == null)
            {
                return Spawn(prefab, position, rotation);
            }
        }
        else
        {
            // 3. 队列空了，只能实例化一个新的
            objToSpawn = Instantiate(prefab);
            objToSpawn.name = key; // 保持名字一致，方便回收
            // 放到父物体下整理
            objToSpawn.transform.SetParent(poolParent);
        }

        // 4. 初始化状态
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true); // 激活！

        // ★ 关键：如果物体上有 IPoolObject 接口，调用它的 OnSpawn 方法
        // 这比 OnEnable 更可控
        IPoolObject poolObj = objToSpawn.GetComponent<IPoolObject>();
        if (poolObj != null)
        {
            poolObj.OnSpawn();
        }

        return objToSpawn;
    }

    /// <summary>
    /// 把对象归还给池子
    /// </summary>
    /// <param name="obj">要回收的对象</param>
    public void Despawn(GameObject obj)
    {
        // 名字就是 Key (注意：Instantiate 出来的名字可能会带 "(Clone)"，需要处理一下或者直接用 prefab.name)
        // 这里为了简单，我们在 Spawn 的时候已经把 name 改成 key 了，所以直接用 name
        string key = obj.name;

        // 1. 隐藏
        obj.SetActive(false);

        // 2. 归队
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary.Add(key, new Queue<GameObject>());
        }

        poolDictionary[key].Enqueue(obj);
    }
}

// ★ 定义一个接口：所有需要被池化管理的对象（怪、特效、数字）最好都实现这个接口
// 这样你就不用写 OnEnable 了，而是写 OnSpawn，逻辑更清晰
public interface IPoolObject
{
    void OnSpawn();
}