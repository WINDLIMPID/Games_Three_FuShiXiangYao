using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("引用设置")]
    public Joystick joystick; // 拖入你的 Fixed Joystick 预制体
    public RectTransform joystickRect; // 摇杆的 RectTransform

    private CanvasGroup canvasGroup;
    private Vector2 initialAnchoredPosition;

    void Awake()
    {
        // 尝试获取 CanvasGroup，如果没有就自动添加一个
        canvasGroup = joystick.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = joystick.gameObject.AddComponent<CanvasGroup>();

        // 记录一下初始位置（可选）
        initialAnchoredPosition = joystickRect.anchoredPosition;

        // 初始状态隐藏
        HideJoystick();
    }

    // 当手指按下时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. 将摇杆移动到手指点击的位置
        // 使用 RectTransformUtility 确保坐标转换正确（即使 Canvas 缩放模式不同也能正常工作）
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            // 注意：这里我们将摇杆的位置设置为相对于本触摸区域的局部坐标
            // 假设 joystickRect 是本脚本所在物体的子物体，或者同级物体
            // 如果 joystickRect 是全局的，可能需要调整父级转换
            joystickRect.position = eventData.position;
        }
        else
        {
            joystickRect.position = eventData.position;
        }

        // 2. 显示摇杆
        ShowJoystick();

        // 3. 手动调用插件的按下逻辑，使其立即进入可拖拽状态
        joystick.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        joystick.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystick.OnPointerUp(eventData);
        HideJoystick();
    }

    private void ShowJoystick()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; // 显示时：允许摇杆交互
    }

    private void HideJoystick()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false; // 🔥 关键修复：隐藏时，让射线穿透摇杆，否则下次点击会点到透明的摇杆上！
    }
}