using System;

// 通用的响应结果包装类
[Serializable]
public class ResponseResult<T>
{
    public int code; // 0 为成功
    public string message;
    public T data;
}

// 🔥 报错就是因为缺了这个类
[Serializable]
public class VerifyData
{
    public bool isVerified;
}

// 登录返回的数据
[Serializable]
public class LoginData
{
    public string token;
    public string userType;
    public bool isVerified;
}

// (可选) 如果你的 AccountManager 里用到了 UserInfoData，也保留它
[Serializable]
public class UserInfoData
{
    public string id;
    public string username;
    public string userType;
    public bool isVerified;
    public string idCardName;
    public string idCardNumber;
}