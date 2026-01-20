using UnityEngine;
using TMPro;

public class ComplianceInfo : MonoBehaviour
{
    [Header("=== UI组件 ===")]
    public TextMeshProUGUI infoText; // 拖入主界面底部的文字组件

    [Header("=== 版权数据 ===")]
    public string copyrightOwner = "你的真实姓名或公司名"; // 必须与软著一致
    public string publisherName = "待定出版社";

    void Start()
    {
        UpdateComplianceText();
    }

    public void UpdateComplianceText()
    {
        if (infoText != null)
        {
            // 严格遵循三类版号格式，确保没有英文
            infoText.text =
                $"审批文号：待核发\n" +
                $"出版物号：待核发\n" +
                $"著作权人：{copyrightOwner}\n" +
                $"出版单位：{publisherName}";
        }
    }
}