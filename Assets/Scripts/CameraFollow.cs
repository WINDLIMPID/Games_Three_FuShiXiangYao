using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f; // 稍微调小一点，0.15是个很跟手的数值
    public Vector3 offset;

    private Vector3 _velocity = Vector3.zero;

    void Start()
    {
        if (target != null) offset = transform.position - target.position;
    }

    // ★★★ 必须改回 LateUpdate ★★★
    // 因为开启了插值(Interpolate)的主角，只有在 LateUpdate 里
    // 才能获取到平滑后的准确坐标！
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
    }
}