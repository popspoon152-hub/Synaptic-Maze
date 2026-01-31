using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置 (Movement)")]
    [SerializeField] private float startSpeed = 2f;      // 初始速度
    [SerializeField] private float acceleration = 20f;   // 加速度
    [SerializeField] private float maxSpeed = 8f;        // 最大速度

    [Header("跳跃设置 (Jump Settings)")]
    [SerializeField] private float jumpHeight = 4f;      // 跳跃最高高度
    [SerializeField] private float timeToApex = 0.4f;    // 到达顶点所需时间

    [Header("地面检测 (Ground Check)")]
    [SerializeField] private LayerMask groundLayer;      // 地面图层
    [SerializeField] private Transform groundCheckPoint; // 检测点
    [SerializeField] private float checkRadius = 0.2f;   // 检测半径

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private float jumpVelocity;
    private float gravityStrength;

    // 动画参数
    private Animator anim;
    //private static readonly int IsMoving = Animator.StringToHash("isMoving");
    //private static readonly int VerticalVelocity = Animator.StringToHash("verticalVelocity");
    //private static readonly int OnGround = Animator.StringToHash("isGrounded");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        CalculatePhysics();
    }

    // 当你在Inspector修改数值时，实时更新物理参数
    void OnValidate()
    {
        CalculatePhysics();
    }

    private void CalculatePhysics()
    {
        // 根据高度和时间计算重力与初速度
        // 公式: g = (2 * height) / time^2
        gravityStrength = (2f * jumpHeight) / Mathf.Pow(timeToApex, 2f);
        // 公式: v = g * time
        jumpVelocity = gravityStrength * timeToApex;

        if (rb != null)
        {
            // 将计算出的重力应用到 Rigidbody2D 的 gravityScale
            // Unity默认重力是 Physics2D.gravity.y (通常是 -9.81)
            rb.gravityScale = gravityStrength / Mathf.Abs(Physics2D.gravity.y);
        }
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void HandleInput()
    {
        // 1. 左右移动输入 (支持 A/Z 和 D/X)
        float moveInput = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Z)) moveInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.X)) moveInput = 1f;
        horizontalInput = moveInput;

        // 2. 跳跃输入 (W 或 Space)
        if (isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    private void ApplyMovement()
    {
        float targetSpeed = horizontalInput * maxSpeed;

        // 处理加速度
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // 如果当前速度极低，应用初始速度 startSpeed
            float currentX = rb.velocity.x;
            if (Mathf.Abs(currentX) < startSpeed)
            {
                currentX = horizontalInput * startSpeed;
            }

            // 平滑加速到最高速度
            float newSpeed = Mathf.MoveTowards(currentX, targetSpeed, acceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(newSpeed, rb.velocity.y);
        }
        else
        {
            // 停止时的减速（可以根据需要添加摩擦力参数）
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, acceleration * Time.fixedDeltaTime), rb.velocity.y);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer);
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        //anim.SetBool(IsMoving, Mathf.Abs(horizontalInput) > 0.1f);
        //anim.SetBool(OnGround, isGrounded);
        //anim.SetFloat(VerticalVelocity, rb.velocity.y);

        // 角色转向
        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // 在编辑器中绘制检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        }
    }
}