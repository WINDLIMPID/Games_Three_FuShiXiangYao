using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("参数设置")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 15f;
    public float expMultiplier = 1.0f;

    [Header("组件引用")]
    public Joystick joystick;
    public Animator animator;

    private Rigidbody _rb;
    private Vector3 _moveVector;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponent<Animator>();

        // 自动找摇杆 (防呆设计)
        if (joystick == null) joystick = FindObjectOfType<Joystick>();

        // 读取全局配置
        if (GlobalConfig.Instance != null)
        {
            if (GlobalConfig.Instance.initialMoveSpeed > 0)
                moveSpeed = GlobalConfig.Instance.initialMoveSpeed;
            expMultiplier = GlobalConfig.Instance.initialExpMultiplier;
        }

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    void Update()
    {
        HandleInput();
        HandleRotation();
        HandleAnimation();
    }

    void FixedUpdate()
    {
        HandlePhysicsMovement();
    }

    void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (joystick != null)
        {
            if (Mathf.Abs(joystick.Horizontal) > 0.1f || Mathf.Abs(joystick.Vertical) > 0.1f)
            {
                h = joystick.Horizontal;
                v = joystick.Vertical;
            }
        }
        _moveVector = new Vector3(h, 0, v).normalized;
    }

    void HandlePhysicsMovement()
    {
        if (_moveVector.magnitude >= 0.1f)
        {
            // 直接移动，不再检测悬崖
            _rb.velocity = new Vector3(_moveVector.x * moveSpeed, _rb.velocity.y, _moveVector.z * moveSpeed);
        }
        else
        {
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        }
    }

    void HandleRotation()
    {
        if (_moveVector.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveVector);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", _moveVector.magnitude);
        }
    }
}