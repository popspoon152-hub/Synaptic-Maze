using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    #region Walk && Jump Parameters
    [Header("移动设置 (Movement)")]
    public float startSpeed = 2f;      // 初始速度
    public float acceleration = 20f;   // 加速度
    public float maxSpeed = 8f;        // 最大速度

    [Header("跳跃设置 (Jump Settings)")]
    public float jumpHeight = 4f;      // 跳跃最高高度
    public float timeToApex = 0.4f;    // 到达顶点所需时间

    [Header("地面检测 (Ground Check)")]
    public LayerMask groundLayer;      // 地面图层
    public Transform groundCheckPoint; // 检测点
    public float checkRadius = 0.2f;   // 检测半径

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

    [SerializeField] private SpriteRenderer headMaskRenderer; //头上戴的
    [Header("开局设置")]
    [SerializeField] private GameObject startMaskPrefab;    //开局时戴的面具预制体


    private PlayerMask currentMaskInstance; // 存储当前逻辑组件
    private GameObject currentMaskPrefab;    // 存储当前面具的预制体引用，用于交换时重新生成
    private bool hasMask = false;

    #endregion

    #region SportsMaskParameters

    [Header("SportsMask 砖块交互")]
    [SerializeField] private LayerMask brickLayer;
    [SerializeField] private float smashRadius = 1.0f;

    #endregion

    #region Animations
    // 动画参数
    private Animator anim;
    private string currentAnim;
    //private static readonly int IsMoving = Animator.StringToHash("isMoving");
    //private static readonly int VerticalVelocity = Animator.StringToHash("verticalVelocity");
    //private static readonly int OnGround = Animator.StringToHash("isGrounded");

    #endregion

    #region LifeCycle  Input && Physics && SportsMask
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        CalculatePhysics();
    }

    void Start()
    {
        if (startMaskPrefab != null)
        {
            // 获取预制体上的 PlayerMask 组件
            if (startMaskPrefab.TryGetComponent<PlayerMask>(out var maskData))
            {
                ApplyInitialMask(maskData);
            }
        }
    }

    // 当你在Inspector修改数值时，实时更新物理参数
    void OnValidate()
    {
        CalculatePhysics();
    }

    public void CalculatePhysics()
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //检查是否戴着面具且是 SportsMask 类型
        if (hasMask && currentMaskInstance != null && currentMaskInstance.maskType == MaskType.SportsMask)
        {
            //检查是否正在下落（纵向速度为负)
            if (rb.velocity.y < -0.1f)
            {
                //检查碰撞物体是否在砖块层级
                if (((1 << collision.gameObject.layer) & brickLayer) != 0)
                {
                    HandleBrickDestruction(collision);
                }
            }
        }
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

    #region CheckInteraction

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.E))
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


        if (headMaskRenderer != null)
        {
            headMaskRenderer.sprite = currentMaskInstance.MaskSprite;
            headMaskRenderer.enabled = true;
        }

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

                if (headMaskRenderer != null)
                {
                    headMaskRenderer.sprite = null;
                    headMaskRenderer.enabled = false;
                }

                hasMask = false;
            }
        }
    }
    #endregion

    #region StartMask

    private void ApplyInitialMask(PlayerMask maskData)
    {
        currentMaskInstance = gameObject.AddComponent(maskData.GetType()) as PlayerMask;
        CopyMaskProperties(maskData, currentMaskInstance);
        currentMaskInstance.ApplyEffect(this);
        hasMask = true;

        if (headMaskRenderer != null && maskData != null)
        {
            headMaskRenderer.sprite = maskData.MaskSprite;
            headMaskRenderer.enabled = true; // 确保显示
        }
    }

    #endregion

    #region SportsMask

    private void HandleBrickDestruction(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<UnityEngine.Tilemaps.Tilemap>(out var tilemap))
        {
            // 以第一个碰撞点为中心进行范围销毁
            Vector3 smashCenter = collision.contacts[0].point;

            // 遍历以碰撞点为中心的矩形区域
            for (float x = -smashRadius; x <= smashRadius; x += 0.5f)
            {
                for (float y = -smashRadius; y <= smashRadius; y += 0.5f)
                {
                    Vector3 checkPos = smashCenter + new Vector3(x, y, 0);
                    Vector3Int cellPos = tilemap.WorldToCell(checkPos);

                    if (tilemap.HasTile(cellPos))
                    {
                        tilemap.SetTile(cellPos, null);
                    }
                }
            }
        }
    }

    #endregion

    #region UpdateAnimations

    private void UpdateAnimations()
    {
        string nextAnim;

        if (!isGrounded)
        {
            // 只要离地，就播跳跃
            nextAnim = "Jump";
        }
        else if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // 在地且移动，播走路
            nextAnim = "Walk";
        }
        else
        {
            // 在地且不动，播待机
            nextAnim = "Idle";
        }

        // 只有当动画改变时才调用 Play，避免每帧重置动画导致“卡死在第一帧”
        if (currentAnim != nextAnim)
        {
            anim.Play(nextAnim);
            currentAnim = nextAnim;
        }

        // 转向逻辑保持不变
        if (horizontalInput != 0)
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
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