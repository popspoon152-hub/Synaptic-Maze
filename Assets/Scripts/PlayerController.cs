using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 输入控制
    private float moveInput;
    private bool isJumpPressed;
    
    // 物理组件
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D collider;
    
    // 地面检测
    [Header("地面检测")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    private bool isGrounded;
    
    // 移动参数
    [Header("移动参数")]
    [SerializeField] private float moveInitialVelocity = 5f;
    [SerializeField] private float moveAcceleration = 20f;
    [SerializeField] private float maxMoveSpeed = 10f;
    
    // 跳跃参数
    [Header("跳跃参数")]
    [SerializeField] private float jumpMaxHeight = 3f;
    [SerializeField] private float jumpTimeToApex = 0.5f;
    private float jumpForce;
    private float gravityScale;
    private float originalGravityScale;
    
    // 动画参数
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
        originalGravityScale = rb.gravityScale;
        CalculateJumpParameters();
    }
    
    private void Update()
    {
        // 检测地面
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 输入处理
        HandleInput();
        
        // 更新动画参数
        UpdateAnimationParameters();
    }
    
    private void FixedUpdate()
    {
        // 移动处理
        HandleMovement();
        
        // 跳跃处理
        HandleJump();
    }
    
    private void HandleInput()
    {
        // 水平移动输入 (A/Z 向左, D/X 向右)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Z))
        {
            moveInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.X))
        {
            moveInput = 1f;
        }
        else
        {
            moveInput = horizontalInput;
        }
        
        // 跳跃输入 (W 键或空格键)
        isJumpPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);
    }
    
    private void HandleMovement()
    {
        if (moveInput != 0)
        {
            // 基于加速度的平滑移动
            float currentSpeed = rb.velocity.x;
            float targetSpeed = moveInput * maxMoveSpeed;
            
            // 计算加速度
            float acceleration = moveInput * moveAcceleration * Time.fixedDeltaTime;
            
            // 应用加速度
            float newSpeed = currentSpeed + acceleration;
            
            // 限制最大速度
            newSpeed = Mathf.Clamp(newSpeed, -maxMoveSpeed, maxMoveSpeed);
            
            // 应用新速度
            rb.velocity = new Vector2(newSpeed, rb.velocity.y);
        }
        else
        {
            // 没有输入时，逐渐减速
            rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0f, 0.1f), rb.velocity.y);
        }
    }
    
    private void HandleJump()
    {
        if (isGrounded && isJumpPressed)
        {
            // 应用跳跃力
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    
    private void CalculateJumpParameters()
    {
        // 计算重力加速度
        gravityScale = (2 * jumpMaxHeight) / (jumpTimeToApex * jumpTimeToApex);
        
        // 计算跳跃力
        jumpForce = (2 * jumpMaxHeight) / jumpTimeToApex;
        
        // 应用重力缩放
        rb.gravityScale = gravityScale;
    }
    
    private void UpdateAnimationParameters()
    {
        // 更新移动动画参数
        animator.SetBool(IsMoving, Mathf.Abs(rb.velocity.x) > 0.1f);
        
        // 更新跳跃动画参数
        animator.SetBool(IsJumping, !isGrounded && rb.velocity.y > 0.1f);
        
        // 更新地面检测动画参数
        animator.SetBool(IsGrounded, isGrounded);
    }
    
    // 当跳跃参数变化时重新计算
    private void OnValidate()
    {
        CalculateJumpParameters();
    }
    
    // 绘制地面检测范围
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}