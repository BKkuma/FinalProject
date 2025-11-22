using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform firePoint;
    public Transform crouchFirePoint;

    [Header("Crouch Settings")]
    [Range(0.1f, 1f)]
    public float crouchHeightMultiplier = 0.8f;

    [Header("Effects")]
    public GameObject jumpEffectPrefab;

    // ⭐ แก้ไข: ตัวแปรล็อคการควบคุมการรับ Input
    [HideInInspector]
    public bool isLocked = false;

    private Rigidbody2D rb;
    private Vector2 shootDirection = Vector2.right;
    private int jumpCount = 0;
    public int maxJumps = 2;

    private bool isCrouching = false;
    private bool isAimingVertical = false;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private float originalHeight;
    private Vector2 originalOffset;
    private Animator animator;

    // Properties สำหรับเข้าถึงจากภายนอก
    public bool IsCrouching => isCrouching;
    public Vector2 ShootDirection => shootDirection;
    public bool IsFacingRight => !spriteRenderer.flipX;

    // ----------------------------------------------------
    // Start & Update
    // ----------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        originalHeight = boxCollider.size.y;
        originalOffset = boxCollider.offset;

        shootDirection = Vector2.right;
    }

    void Update()
    {
        // ⭐ NEW: ล็อคการรับ Input ทั้งหมดถ้า isLocked เป็น true
        if (isLocked)
            return;

        AimDirection();
        Move();
        Jump();
        Crouch();
    }

    // ----------------------------------------------------
    // Movement & Actions
    // ----------------------------------------------------

    void Move()
    {
        float moveInput = 0;

        if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1;
            if (!isAimingVertical)
            {
                shootDirection = Vector2.left;
                spriteRenderer.flipX = true;
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1;
            if (!isAimingVertical)
            {
                shootDirection = Vector2.right;
                spriteRenderer.flipX = false;
            }
        }

        float horizontalVelocity = moveInput * moveSpeed;

        if (isCrouching)
        {
            horizontalVelocity = 0;
            rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);

            animator.SetFloat("Speed", 0);
            animator.SetBool("isCrouchIdle", true);
            animator.SetBool("isCrouchWalking", false);
        }
        else
        {
            if (isAimingVertical)
            {
                rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
                animator.SetFloat("Speed", 0);
            }
            else
            {
                rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
                animator.SetFloat("Speed", Mathf.Abs(moveInput));
            }

            animator.SetBool("isCrouchIdle", false);
            animator.SetBool("isCrouchWalking", false);
        }
    }


    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.J) && jumpCount < maxJumps && !isCrouching)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;

            animator.SetBool("isJumping", true);

            if (jumpEffectPrefab != null)
            {
                Vector3 effectPosition = transform.position + new Vector3(0, -boxCollider.size.y / 2f, 0);
                GameObject effect = Instantiate(jumpEffectPrefab, effectPosition, Quaternion.identity);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effect, ps.main.duration);
                }
            }
        }
    }

    void AimDirection()
    {
        bool aimingUp = Input.GetKey(KeyCode.W);
        bool aimingDown = Input.GetKey(KeyCode.S);

        isAimingVertical = false;

        if (aimingUp && !isCrouching)
        {
            shootDirection = Vector2.up;
            animator.SetBool("isAimingUp", true);
            animator.SetBool("isAimingDown", false);
            isAimingVertical = true;
        }
        else if (aimingDown && !isCrouching)
        {
            shootDirection = Vector2.down;
            animator.SetBool("isAimingDown", true);
            animator.SetBool("isAimingUp", false);
            isAimingVertical = true;
        }
        else
        {
            animator.SetBool("isAimingUp", false);
            animator.SetBool("isAimingDown", false);

            if (!isAimingVertical)
            {
                shootDirection = IsFacingRight ? Vector2.right : Vector2.left;
            }
        }
    }

    void Crouch()
    {
        // **1. CROUCH DOWN / STAY CROUCHED**
        if (Input.GetKey(KeyCode.LeftShift) && IsGrounded())
        {
            if (!isCrouching)
            {
                isCrouching = true;

                float crouchHeight = originalHeight * crouchHeightMultiplier;

                boxCollider.size = new Vector2(boxCollider.size.x, crouchHeight);
                boxCollider.offset = new Vector2(originalOffset.x, originalOffset.y - (originalHeight - crouchHeight) / 2f);

                animator.SetBool("isAimingUp", false);
                animator.SetBool("isAimingDown", false);
                isAimingVertical = false;

                shootDirection = IsFacingRight ? Vector2.right : Vector2.left;
            }
        }

        // **2. STAND UP TRIGGER (เมื่อปล่อยปุ่ม)**
        if (Input.GetKeyUp(KeyCode.LeftShift) && isCrouching)
        {
            isCrouching = false;

            boxCollider.size = new Vector2(boxCollider.size.x, originalHeight);
            boxCollider.offset = originalOffset;

            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isCrouchIdle", false);
        }

        // **3. FORCED STAND UP (เมื่อตกจากขอบขณะย่อตัว)**
        if (isCrouching && !IsGrounded() && !Input.GetKey(KeyCode.LeftShift))
        {
            isCrouching = false;

            boxCollider.size = new Vector2(boxCollider.size.x, originalHeight);
            boxCollider.offset = originalOffset;

            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isCrouchIdle", false);
        }
    }

    // ----------------------------------------------------
    // Collision & Ground Check
    // ----------------------------------------------------

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal == Vector2.up)
        {
            jumpCount = 0;
            animator.SetBool("isJumping", false);
        }

        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }

    public bool IsGrounded()
    {
        float extraHeight = 0.1f;
        int layerMask = LayerMask.GetMask("Ground");

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            (boxCollider.size.y / 2) + extraHeight,
            layerMask
        );

        Debug.DrawRay(transform.position, Vector2.down * ((boxCollider.size.y / 2) + extraHeight), hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }
}