using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance;
    private string baseUrl = "http://lmgame.top:40004/api";
    private string appKey = "APP_204151C4";

    private const string PREF_LAST_USER = "Account_LastUsername";
    private const string PREF_TOKEN = "Account_Token";
    private const string PREF_LOCAL_PWD_PREFIX = "LocalPwd_";

    public string token;
    public bool isLoggedIn = false;
    public bool isOfflineMode = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        token = PlayerPrefs.GetString(PREF_TOKEN, "");
    }

    public string GetLastUsedUsername()
    {
        return PlayerPrefs.GetString(PREF_LAST_USER, "");
    }

    public string GetLocalPassword(string username)
    {
        if (string.IsNullOrEmpty(username)) return "";
        return PlayerPrefs.GetString(PREF_LOCAL_PWD_PREFIX + username, "");
    }

    private void SaveLocalCredentials(string username, string password)
    {
        PlayerPrefs.SetString(PREF_LOCAL_PWD_PREFIX + username, password);
        PlayerPrefs.SetString(PREF_LAST_USER, username);
        PlayerPrefs.Save();
    }

    private bool CheckLocalLogin(string username, string password)
    {
        string savedPass = PlayerPrefs.GetString(PREF_LOCAL_PWD_PREFIX + username, "");
        return (!string.IsNullOrEmpty(savedPass) && savedPass == password);
    }

    // --- 注册逻辑 ---
    public void Register(string username, string password, Action<bool, string> callback)
    {
        StartCoroutine(PostRegister(username, password, callback));
    }

    private IEnumerator PostRegister(string username, string password, Action<bool, string> callback)
    {
        string url = baseUrl + "/user/register";
        string json = $"{{\"appKey\":\"{appKey}\",\"username\":\"{username}\",\"password\":\"{password}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 3; // 🔥 设置较短的超时时间，3秒连不上就转本地

            yield return request.SendWebRequest();

            // 1. 情况一：服务器成功响应
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ResponseResult<object>>(request.downloadHandler.text);
                if (response.code == 0)
                {
                    // 注册成功，这里我们也顺手存一份本地，防止玩家以后断网没法登
                    SaveLocalCredentials(username, password);
                    callback?.Invoke(true, "注册成功");
                }
                else
                {
                    // 服务器明确拒绝（比如：账号已存在），这种情况下【不能】转本地，必须报错
                    callback?.Invoke(false, response.message);
                }
            }
            // 2. 情况二：网络连接失败 (断网、超时、服务器挂了)
            else
            {
                Debug.LogWarning("⚠️ 网络请求失败，尝试转为本地离线注册...");

                // 检查本地是不是已经有这个号了（防止覆盖旧密码）
                string existingPwd = GetLocalPassword(username);
                if (!string.IsNullOrEmpty(existingPwd))
                {
                    callback?.Invoke(false, "本地账号已存在，请直接登录");
                }
                else
                {
                    // 🔥🔥🔥 核心修改：网络失败 -> 强制本地注册成功 🔥🔥🔥
                    SaveLocalCredentials(username, password);

                    // 告诉 UI 注册成功了，但提示是离线模式
                    callback?.Invoke(true, "网络未连接，已注册为离线账号");
                }
            }
        }
    }

    // --- 登录逻辑 ---
    public void Login(string username, string password, Action<bool, string> callback)
    {
        StartCoroutine(PostLogin(username, password, callback));
    }

    private IEnumerator PostLogin(string username, string password, Action<bool, string> callback)
    {
        string url = baseUrl + "/user/login";
        string json = $"{{\"appKey\":\"{appKey}\",\"username\":\"{username}\",\"password\":\"{password}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 3; // 登录也缩短超时

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ResponseResult<LoginData>>(request.downloadHandler.text);
                if (response.code == 0)
                {
                    // 联网登录成功
                    isLoggedIn = true;
                    isOfflineMode = false;
                    token = response.data.token;
                    SaveLocalCredentials(username, password);
                    PlayerPrefs.SetString(PREF_TOKEN, token);

                    if (AntiAddictionManager.Instance != null)
                    {
                        bool serverVerified = response.data.isVerified;
                        bool localVerified = AntiAddictionManager.Instance.CheckLocalVerifyStatus(username);
                        bool finalVerified = serverVerified || localVerified;

                        AntiAddictionManager.Instance.isVerified = finalVerified;
                        AntiAddictionManager.Instance.SaveLocalVerifyStatus(username, finalVerified);
                    }

                    callback?.Invoke(true, "登录成功");
                }
                else
                {
                    // 服务器返回密码错误等业务逻辑错误，不应该尝试离线登录
                    callback?.Invoke(false, response.message);
                }
            }
            else
            {
                // 网络连接失败 -> 尝试离线登录
                if (CheckLocalLogin(username, password))
                {
                    isLoggedIn = true;
                    isOfflineMode = true;

                    if (AntiAddictionManager.Instance != null)
                    {
                        bool localVerified = AntiAddictionManager.Instance.CheckLocalVerifyStatus(username);
                        AntiAddictionManager.Instance.isVerified = localVerified;
                    }
                    callback?.Invoke(true, "离线登录成功");
                }
                else
                {
                    callback?.Invoke(false, "登录失败：网络异常且无本地存档");
                }
            }
        }
    }

    public void VerifyToken(Action<bool, string> callback) { }
    public void ClearToken() { token = ""; PlayerPrefs.DeleteKey(PREF_TOKEN); }
}