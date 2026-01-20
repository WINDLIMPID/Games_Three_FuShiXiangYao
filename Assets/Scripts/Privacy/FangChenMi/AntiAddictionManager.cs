using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AntiAddictionManager : MonoBehaviour
{
    public static AntiAddictionManager Instance;
    private string baseUrl = "http://lmgame.top:40004/api";
    private string appKey = "APP_204151C4";

    [Header("当前运行时的状态")]
    public bool isVerified = false; // 当前登录账号是否已实名

    // 🔥 调试开关：勾选后编辑器也能发真实请求
    public bool useRealServerInEditor = false;

    // 🔥 核心修改：定义前缀，后面会拼接账号名
    private const string PREF_VERIFY_PREFIX = "Verified_";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// 🔥 保存该账号的实名状态到本地 (多账号隔离的关键)
    /// </summary>
    public void SaveLocalVerifyStatus(string username, bool verified)
    {
        if (string.IsNullOrEmpty(username)) return;

        // Key 变成了 "Verified_张三"
        PlayerPrefs.SetInt(PREF_VERIFY_PREFIX + username, verified ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"📝 [实名存档] 账号 {username} 实名状态已保存: {verified}");
    }

    /// <summary>
    /// 🔥 从本地读取该账号是否已实名
    /// </summary>
    public bool CheckLocalVerifyStatus(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;

        // 读取 "Verified_张三"
        return PlayerPrefs.GetInt(PREF_VERIFY_PREFIX + username, 0) == 1;
    }

    // 发起实名认证请求
    public void RequestVerify(string realName, string idCard, string username, System.Action<bool> callback)
    {
        bool shouldRunRealRequest = false;

        // 真机环境 或 开启了调试模式，都走真实服务器
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor)
        {
            shouldRunRealRequest = true;
        }
        if (useRealServerInEditor) shouldRunRealRequest = true;

        if (shouldRunRealRequest)
        {
            StartCoroutine(PostVerify(realName, idCard, username, callback));
        }
        else
        {
            Debug.LogWarning("⚠️ [模拟认证] 编辑器模式默认通过。如需测试真实请求请勾选 useRealServerInEditor");

            // 模拟成功：也要更新内存和本地存档
            isVerified = true;
            SaveLocalVerifyStatus(username, true);
            callback?.Invoke(true);
        }
    }

    private IEnumerator PostVerify(string name, string idCard, string username, System.Action<bool> callback)
    {
        string url = baseUrl + "/user/verify";
        string json = $"{{\"appKey\":\"{appKey}\",\"username\":\"{username}\",\"idCardName\":\"{name}\",\"idCardNumber\":\"{idCard}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                VerifyData data = null;
                try
                {
                    var response = JsonUtility.FromJson<ResponseResult<VerifyData>>(request.downloadHandler.text);
                    if (response != null && response.code == 0) data = response.data;
                }
                catch { }

                if (data != null)
                {
                    // 🔥 认证成功：更新状态并存档
                    isVerified = true;
                    SaveLocalVerifyStatus(username, true);
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError("❌ 实名认证失败，服务器返回错误");
                    callback?.Invoke(false);
                }
            }
            else
            {
                Debug.LogError("❌ 网络请求失败: " + request.error);
                callback?.Invoke(false);
            }
        }
    }
}