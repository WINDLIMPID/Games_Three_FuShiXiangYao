using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraFinder : MonoBehaviour
{
    private Canvas _canvas;

    [Header("设置")]
    [Tooltip("Canvas 与相机的距离，建议 100，确保能容纳粒子特效")]
    public float planeDistance = 100f;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    void OnEnable()
    {
        // 1. 注册场景加载事件（每当切场景时，尝试找一次相机）
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryAssignCamera();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignCamera();
    }

    void Update()
    {
        // 🔥 保底逻辑：如果 Canvas 还没绑定相机（因为相机是后来才加载的），就在 Update 里一直找
        // 只要绑定成功了，就不会再执行这里的逻辑，性能消耗极小
        if (_canvas.worldCamera == null)
        {
            TryAssignCamera();
        }
    }

    void TryAssignCamera()
    {
        // 只有当 Canvas 处于 Camera 模式时才需要找
        if (_canvas.renderMode != RenderMode.ScreenSpaceCamera) return;

        // 尝试获取主相机 (前提：你的相机 Tag 必须是 MainCamera)
        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            _canvas.worldCamera = mainCam;
            _canvas.planeDistance = planeDistance;

            // 这一步很重要：刷新一下 Sorting Layer，防止粒子还是被挡住
            _canvas.sortingLayerName = "UI";
            _canvas.sortingOrder = 0; // Canvas 的层级

            Debug.Log($"✅ Canvas 已自动绑定到新相机: {mainCam.name}");
        }
    }
}