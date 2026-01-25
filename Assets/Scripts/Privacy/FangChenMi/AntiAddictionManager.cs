using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;

public class AntiAddictionManager : MonoBehaviour
{
    public static AntiAddictionManager Instance;

    [Header("=== 配置 ===")]
    private string baseUrl = "http://lmgame.top:40004/api";
    private string appKey = "APP_204151C4";
    public bool useRealServerInEditor = false;

    private const string PREF_VERIFY_PREFIX = "Verified_";
    private const string PREF_AGE_PREFIX = "UserAge_";

    [Header("=== 状态 ===")]
    public bool isVerified = false;
    public int currentUserAge = -1;
    public bool isMinor = false;

    [Header("=== 节假日配置 (格式: 2023-10-01) ===")]
    public List<string> legalHolidays = new List<string>();

    private bool hasKickedOut = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // --- 修改 AntiAddictionManager.cs 的 Update 方法 ---

    void Update()
    {
        // 🔥 [开发者测试按键]：按下 T 键直接触发踢人弹窗
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🛠 [测试] 触发手动防沉迷踢人演示");
            // 模拟一个未成年人状态，确保能触发逻辑
            isVerified = true;
            isMinor = true;

            string kickMsg = "    您好，根据防沉迷规定，未成年人游戏时间已结束（ 20 : 00 - 21 : 00 ）。\n    系统将强制下线，请注意休息。";
            TriggerKickOut(kickMsg);
        }

        // 原有的 21:00 自动监控逻辑
        if (isVerified && isMinor && !hasKickedOut)
        {
            DateTime now = DateTime.Now;
            if (now.Hour >= 21 || now.Hour < 20)
            {
                string kickMsg = "    您好，根据防沉迷规定，未成年人游戏时间已结束（ 20 : 00 - 21 : 00 ）。\n    系统将强制下线，请注意休息。";
                TriggerKickOut(kickMsg);
            }
        }
    }
   
    void TriggerKickOut(string msg)
    {
        hasKickedOut = true;
        Debug.LogError("⛔ [防沉迷] 强制下线: " + msg);

        // 1. 暂停游戏
        Time.timeScale = 0;

        // 2. 呼叫卷轴弹窗
        if (GlobalCanvas.Instance != null)
        {
            GlobalCanvas.Instance.ShowTip(msg, () => {
                // 点击按钮后的操作：退出游戏
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            }, "退出游戏");
        }
    }

    // --- 初始化与认证 ---
    public void InitData(string username)
    {
        hasKickedOut = false;
        bool localStatus = CheckLocalVerifyStatus(username);

        if (localStatus)
        {
            isVerified = true;
            currentUserAge = PlayerPrefs.GetInt(PREF_AGE_PREFIX + username, 0);
            isMinor = currentUserAge < 18;
            Debug.Log($"[防沉迷] 用户 {username} 已实名，年龄: {currentUserAge}");
        }
        else
        {
            isVerified = false;
            currentUserAge = -1;
            isMinor = false;
        }
    }

    public void RequestVerify(string realName, string idCard, string username, System.Action<bool, string> callback)
    {
        int calculatedAge = CalculateAge(idCard);
        if (calculatedAge == -1)
        {
            callback?.Invoke(false, "身份证号格式错误");
            return;
        }

        bool shouldRunRealRequest = (Application.platform != RuntimePlatform.WindowsEditor) || useRealServerInEditor;

        if (shouldRunRealRequest)
        {
            StartCoroutine(PostVerify(realName, idCard, username, calculatedAge, callback));
        }
        else
        {
            Debug.Log($"[模拟认证] 身份证: {idCard}, 计算年龄: {calculatedAge}");
            OnVerifySuccess(username, calculatedAge);
            callback?.Invoke(true, "认证成功");
        }
    }

    private IEnumerator PostVerify(string name, string idCard, string username, int age, System.Action<bool, string> callback)
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
                OnVerifySuccess(username, age);
                callback?.Invoke(true, "认证成功");
            }
            else
            {
                Debug.LogError("实名认证失败: " + request.error);
                callback?.Invoke(false, "认证服务连接失败");
            }
        }
    }

    private void OnVerifySuccess(string username, int age)
    {
        isVerified = true;
        currentUserAge = age;
        isMinor = age < 18;
        SaveLocalVerifyStatus(username, true, age);
    }

    // --- 🔥 核心限制逻辑：返回带排版的提示语 ---
    public string CheckLoginLimit(int age)
    {
        if (age < 0) return null;

        // 规则一：16岁以下不能登录
        if (age < 16)
        {
            // 单行短句，UI会自动居中
            return "根据防沉迷规定，本游戏未满 16 周岁无法登录。";
        }

        // 规则二：16-17岁限制
        if (age >= 16 && age < 18)
        {
            DateTime now = DateTime.Now;
            bool isWeekend = (now.DayOfWeek == DayOfWeek.Friday || now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday);
            string todayStr = now.ToString("yyyy-MM-dd");
            bool isHoliday = legalHolidays.Contains(todayStr);

            bool isAllowDay = isWeekend || isHoliday;
            bool isAllowTime = (now.Hour == 20); // 只有 20:00 - 20:59 是允许的

            // 只要条件不满足，就弹窗
            if (!isAllowDay || !isAllowTime)
            {
                // 🔥 多行长句，UI会自动左对齐
                return "当前时段为非游戏服务时间，未成年人无法登录\n\n请于周五、周六、周日及法定节假日20 : 00 至 21 : 00 登录游戏。";
            }
        }

        // 18岁以上无限制
        return null;
    }

    // --- 辅助方法 ---
    public void SaveLocalVerifyStatus(string username, bool verified, int age)
    {
        if (string.IsNullOrEmpty(username)) return;
        PlayerPrefs.SetInt(PREF_VERIFY_PREFIX + username, verified ? 1 : 0);
        PlayerPrefs.SetInt(PREF_AGE_PREFIX + username, age);
        PlayerPrefs.Save();
    }

    public bool CheckLocalVerifyStatus(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        return PlayerPrefs.GetInt(PREF_VERIFY_PREFIX + username, 0) == 1;
    }

    public int CalculateAge(string idCard)
    {
        if (string.IsNullOrEmpty(idCard) || (idCard.Length != 15 && idCard.Length != 18)) return -1;
        string birthStr = "";
        try
        {
            if (idCard.Length == 18) birthStr = idCard.Substring(6, 8);
            else if (idCard.Length == 15) birthStr = "19" + idCard.Substring(6, 6);

            int year = int.Parse(birthStr.Substring(0, 4));
            int month = int.Parse(birthStr.Substring(4, 2));
            int day = int.Parse(birthStr.Substring(6, 2));

            DateTime birthDate = new DateTime(year, month, day);
            DateTime now = DateTime.Now;

            int age = now.Year - birthDate.Year;
            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
            {
                age--;
            }
            return age;
        }
        catch
        {
            return -1;
        }
    }
}