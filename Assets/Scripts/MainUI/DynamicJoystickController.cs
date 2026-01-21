using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("引用设置")]
    public Joystick joystick; // 拖入你的 Fixed Joystick 预制体
    public RectTransform joystickRect; // 摇杆的 RectTransform

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // 尝试获取 CanvasGroup，如果没有就自动添加一个
        canvasGroup = joystick.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = joystick.gameObject.AddComponent<CanvasGroup>();

        // 初始状态隐藏
        HideJoystick();
    }

    // 当手指按下时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. 将摇杆移动到手指点击的位置
        // 注意：如果你的 UI 缩放模式不同，可能需要用 RectTransformUtility.ScreenPointToLocalPointInRectangle 进行转换
        // 这里假设 TouchArea 和 Joystick 在同一个 Canvas 下且没有复杂的缩放差异
        joystickRect.position = eventData.position;

        // 2. 显示摇杆
        ShowJoystick();

        // 3. 手动调用插件的按下逻辑，使其立即进入可拖拽状态
        joystick.OnPointerDown(eventData);
    }

    // 必须实现此接口，否则 OnPointerDown 后的拖拽可能无法传导给插件
    public void OnDrag(PointerEventData eventData)
    {
        joystick.OnDrag(eventData);
    }

    // 手指抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        joystick.OnPointerUp(eventData);
        HideJoystick();
    }

    private void ShowJoystick()
    {
        canvasGroup.alpha = 1f;
    }

    private void HideJoystick()
    {
        canvasGroup.alpha = 0f;
    }
}