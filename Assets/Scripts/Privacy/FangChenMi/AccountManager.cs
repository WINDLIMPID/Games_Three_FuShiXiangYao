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
    public string currentUsername = "";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
        token = PlayerPrefs.GetString(PREF_TOKEN, "");
    }

    public string GetLastUsedUsername() => PlayerPrefs.GetString(PREF_LAST_USER, "");
    public string GetLocalPassword(string u) => string.IsNullOrEmpty(u) ? "" : PlayerPrefs.GetString(PREF_LOCAL_PWD_PREFIX + u, "");

    private void SaveLocalCredentials(string u, string p)
    {
        PlayerPrefs.SetString(PREF_LOCAL_PWD_PREFIX + u, p);
        PlayerPrefs.SetString(PREF_LAST_USER, u);
        PlayerPrefs.Save();
    }

    private bool CheckLocalLogin(string u, string p)
    {
        string savedPass = PlayerPrefs.GetString(PREF_LOCAL_PWD_PREFIX + u, "");
        return (!string.IsNullOrEmpty(savedPass) && savedPass == p);
    }

    public void Register(string u, string p, Action<bool, string> cb) => StartCoroutine(PostRegister(u, p, cb));

    private IEnumerator PostRegister(string u, string p, Action<bool, string> cb)
    {
        string url = baseUrl + "/user/register";
        string json = $"{{\"appKey\":\"{appKey}\",\"username\":\"{u}\",\"password\":\"{p}\"}}";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 3;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) { SaveLocalCredentials(u, p); cb?.Invoke(true, "注册成功"); }
            else { SaveLocalCredentials(u, p); cb?.Invoke(true, "网络未连接，已注册为离线账号"); }
        }
    }

    public void Login(string username, string password, Action<bool, string> callback)
    {
        // 1. 检查预设测试账号
        if (ComplianceDataManager.Instance != null)
        {
            var testAcc = ComplianceDataManager.Instance.GetTestAccount(username, password);
            if (testAcc != null)
            {
                // 🔥🔥 核心修复：如果检测到是预设账号，立刻强制写入本地实名存档 🔥🔥
                if (AntiAddictionManager.Instance != null)
                {
                    // 1. 检查是否有防沉迷限制（16岁以下或非开放时间）
                    string limitMsg = AntiAddictionManager.Instance.CheckLoginLimit(testAcc.age);
                    if (!string.IsNullOrEmpty(limitMsg))
                    {
                        if (GlobalCanvas.Instance != null) GlobalCanvas.Instance.ShowTip(limitMsg, null, "我知道了");
                        callback?.Invoke(false, "");
                        return;
                    }

                    // 2. 强制保存实名状态到本地，这样下次就不会弹出实名界面了
                    AntiAddictionManager.Instance.SaveLocalVerifyStatus(username, true, testAcc.age);

                    // 3. 同步运行时内存数据
                    AntiAddictionManager.Instance.isVerified = true;
                    AntiAddictionManager.Instance.currentUserAge = testAcc.age;
                    AntiAddictionManager.Instance.isMinor = testAcc.age < 18;
                }

                isLoggedIn = true;
                isOfflineMode = true;
                currentUsername = username;
                SaveLocalCredentials(username, password);

                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.LoadUserData(username);
                    SaveManager.Instance.ApplyTestAccountConfig(testAcc.tier);
                }
                callback?.Invoke(true, "测试账号登录成功");
                return;
            }
        }

        // 2. 普通账号登录
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
            request.timeout = 3;
            yield return request.SendWebRequest();

            bool loginSuccess = false;
            string msg = "";

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ResponseResult<LoginData>>(request.downloadHandler.text);
                if (response.code == 0) { loginSuccess = true; isLoggedIn = true; token = response.data.token; currentUsername = username; msg = "登录成功"; }
                else msg = response.message;
            }
            else
            {
                if (CheckLocalLogin(username, password)) { loginSuccess = true; isLoggedIn = true; isOfflineMode = true; currentUsername = username; msg = "离线登录成功"; }
                else msg = "登录失败：网络异常且无本地存档";
            }

            if (loginSuccess)
            {
                SaveLocalCredentials(username, password);
                // 🔥 初始化防沉迷数据（如果是已认证的普通账号，这里会读取到状态）
                if (AntiAddictionManager.Instance != null) AntiAddictionManager.Instance.InitData(username);
                if (SaveManager.Instance != null) SaveManager.Instance.LoadUserData(username);
                callback?.Invoke(true, msg);
            }
            else callback?.Invoke(false, msg);
        }
    }

    public void VerifyToken(Action<bool, string> cb) { }
    public void ClearToken() { token = ""; PlayerPrefs.DeleteKey(PREF_TOKEN); }
}