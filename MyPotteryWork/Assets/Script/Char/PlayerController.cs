using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // 单例实例
    public static PlayerController Instance { get; private set; }

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float flipThreshold = 0.1f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private Rigidbody rb;
    private float currentSpeed;
    private bool facingRight = true;
    private Vector3 originalScale;
    private Animator animator;
    private bool canMove = true; // 私有字段

    public bool IsSprinting { get; private set; }
    public float CurrentHorizontalSpeed { get; private set; }

    // 添加公共属性访问 canMove
    public bool CanMove => canMove;

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
        currentSpeed = moveSpeed;

        // 锁定刚体旋转
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (!canMove) return;

        HandleMovementInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        MoveCharacter();
    }

    private void HandleMovementInput()
    {
        IsSprinting = Input.GetKey(sprintKey);
        UpdateMovementSpeed();
        UpdateCharacterDirection();
    }

    private void UpdateMovementSpeed()
    {
        float targetSpeed = IsSprinting ? sprintSpeed : moveSpeed;
        float accelerationFactor = targetSpeed > currentSpeed ? acceleration : deceleration;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, accelerationFactor * Time.deltaTime);
    }

    private void UpdateCharacterDirection()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontalInput) < flipThreshold) return;

        bool shouldFaceRight = horizontalInput > 0;
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            ApplyFacingDirection();
        }
    }

    private void ApplyFacingDirection()
    {
        Vector3 newScale = originalScale;
        newScale.x = Mathf.Abs(newScale.x) * (facingRight ? 1 : -1);
        transform.localScale = newScale;

        // 确保不旋转整个对象
        transform.rotation = Quaternion.identity;
    }

    private void MoveCharacter()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.velocity.y;
        rb.velocity = targetVelocity;

        CurrentHorizontalSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", CurrentHorizontalSpeed);
        animator.SetBool("IsSprinting", IsSprinting);
    }

    // 移动控制方法
    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
        rb.velocity = Vector3.zero; // 停止移动
    }
}