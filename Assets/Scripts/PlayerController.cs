using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    #region Walk && Jump Parameters
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

    #endregion

    #region MaskParameters

    [Header("面具交互设置")]
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask maskLayer;       // 设置为 "Mask" 图层
    [SerializeField] private LayerMask doorLayer;       // 设置为 "Door" 图层
    [SerializeField] private Transform maskDropPoint;   // 面具掉落位置（角色头顶或前方）

    private PlayerMask currentMaskInstance; // 存储当前逻辑组件
    private GameObject currentMaskPrefab;    // 存储当前面具的预制体引用，用于交换时重新生成
    private bool hasMask = false;

    #endregion

    #region Animations
    // 动画参数
    private Animator anim;
    //private static readonly int IsMoving = Animator.StringToHash("isMoving");
    //private static readonly int VerticalVelocity = Animator.StringToHash("verticalVelocity");
    //private static readonly int OnGround = Animator.StringToHash("isGrounded");

    #endregion

    #region LifeCycle
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

        gravityStrength = (2f * jumpHeight) / Mathf.Pow(timeToApex, 2f);
        jumpVelocity = gravityStrength * timeToApex;

        if (rb != null)
        {
            rb.gravityScale = gravityStrength / Mathf.Abs(Physics2D.gravity.y);
        }
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        UpdateAnimations();
        HandleInteraction(); // 监听 F 键交互
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    #endregion

    #region Move && Jump
    private void HandleInput()
    {
        float moveInput = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Z)) moveInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.X)) moveInput = 1f;
        horizontalInput = moveInput;

        if (isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    private void ApplyMovement()
    {
        float targetSpeed = horizontalInput * maxSpeed;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            float currentX = rb.velocity.x;
            if (Mathf.Abs(currentX) < startSpeed)
            {
                currentX = horizontalInput * startSpeed;
            }

            float newSpeed = Mathf.MoveTowards(currentX, targetSpeed, acceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(newSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, acceleration * Time.fixedDeltaTime), rb.velocity.y);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, groundLayer);
    }
    #endregion

    #region CheckMask

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, maskLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Mask") && hit.TryGetComponent<PlayerMask>(out var newMask))
                {
                    SwapMask(newMask);
                    return; // 每次只交互一个
                }
            }

            CheckForLock();
        }
    }

    private void SwapMask(PlayerMask newMask)
    {
        // 如果当前已经戴着面具，先把它扔出来
        if (hasMask && currentMaskInstance != null)
        {
            // 移除旧面具效果
            currentMaskInstance.RemoveEffect(this);

            // 在原地重新生成旧面具的预制体
            // 注意：这里需要我们在 PlayerMask 里存一下它自己的原始预制体引用
            Instantiate(currentMaskInstance.originalPrefab, maskDropPoint.position, Quaternion.identity);

            // 销毁当前的逻辑组件
            Destroy(currentMaskInstance);
        }

        // 穿戴新面具
        currentMaskInstance = gameObject.AddComponent(newMask.GetType()) as PlayerMask;

        // 复制属性
        CopyMaskProperties(newMask, currentMaskInstance);

        // 应用效果
        currentMaskInstance.ApplyEffect(this);

        // 销毁场景中的面具物体
        newMask.BeConsumed();

        hasMask = true;

    }

    private void CopyMaskProperties(PlayerMask source, PlayerMask target)
    {
        target.maskType = source.maskType;
        target.maskName = source.maskName;
        target.originalPrefab = source.originalPrefab; // 确保新组件知道自己是谁
        target.maskCoverPrefab = source.maskCoverPrefab;
    }

    private void CheckForLock()
    {
        if (!hasMask || currentMaskInstance == null) return;

        // 在交互范围内寻找层级为 "Door" 的物体
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, doorLayer);

        if (hit != null && hit.TryGetComponent<Door>(out var lockHole))
        {
            // 调用锁孔的验证逻辑
            if (lockHole.TryOpen(currentMaskInstance.maskType))
            {
                // 解锁成功：清理玩家身上的面具效果
                currentMaskInstance.RemoveEffect(this);

                // 销毁逻辑组件并重置状态
                Destroy(currentMaskInstance);
                currentMaskInstance = null;
                hasMask = false; // 允许再次拾取

                // 同步动画状态
                //anim.SetInteger("MaskID", 0);
            }
        }
    }
    #endregion

    #region UpdateAnimations

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

    #endregion

    #region Gizmos
    // 在编辑器中绘制检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        }

        if(maskDropPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(maskDropPoint.position, interactRange);
        }
    }

    #endregion
}