using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform firePoint;
    public Transform crouchFirePoint;

    private Rigidbody2D rb;
    private Vector2 shootDirection = Vector2.right;
    private int jumpCount = 0;
    public int maxJumps = 2;

    private bool isCrouching = false;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider; // ใช้ BoxCollider2D

    private float originalHeight;
    private Vector2 originalOffset;
    private Animator animator;

    public bool IsCrouching => isCrouching;
    public Vector2 ShootDirection => shootDirection;

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
    }

    void Update()
    {
        Move();
        Jump();
        AimDirection();
        Crouch();
    }

    // ----------------------------------------------------
    // Movement & Actions
    // ----------------------------------------------------

    void Move()
    {
        float move = 0;

        if (Input.GetKey(KeyCode.A))
        {
            move = -1;
            shootDirection = Vector2.left;
            spriteRenderer.flipX = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            move = 1;
            shootDirection = Vector2.right;
            spriteRenderer.flipX = false;
        }

        // Animation และการเคลื่อนที่
        if (!isCrouching)
        {
            rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
            animator.SetFloat("Speed", Mathf.Abs(move));
        }
        else // เดินย่อตัว
        {
            rb.velocity = new Vector2(move * (moveSpeed * 0.5f), rb.velocity.y);
            // 🎯 การอัปเดต Animation ถูกย้ายไปอยู่ใน Crouch() เพื่อให้ทำงานขณะกดค้าง
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
        }
    }

    void AimDirection()
    {
        if (Input.GetKey(KeyCode.W))
        {
            shootDirection = Vector2.up;
            animator.SetBool("isAimingUp", true);
            animator.SetBool("isAimingDown", false);
        }
        else if (Input.GetKey(KeyCode.S) && !isCrouching)
        {
            shootDirection = Vector2.down;
            animator.SetBool("isAimingDown", true);
            animator.SetBool("isAimingUp", false);
        }
        else
        {
            shootDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            animator.SetBool("isAimingUp", false);
            animator.SetBool("isAimingDown", false);
        }
    }

    void Crouch()
    {
        // **1. CROUCH DOWN / STAY CROUCHED**
        // 🎯 ใช้ Input.GetKey(KeyCode.LeftShift) เพื่อตรวจจับการกดค้าง
        if (Input.GetKey(KeyCode.LeftShift) && IsGrounded())
        {
            if (!isCrouching) // ทำการปรับขนาด Collider เพียงครั้งเดียวตอนเข้าสู่สถานะย่อตัว
            {
                isCrouching = true;

                float crouchHeight = originalHeight * 0.5f;
                boxCollider.size = new Vector2(boxCollider.size.x, crouchHeight);
                boxCollider.offset = new Vector2(originalOffset.x, originalOffset.y - (originalHeight - crouchHeight) / 2f);
            }

            // อัปเดต Animation ทุกเฟรมขณะย่อ
            animator.SetBool("isCrouchIdle", Mathf.Abs(rb.velocity.x) < 0.1f);
            animator.SetBool("isCrouchWalking", Mathf.Abs(rb.velocity.x) >= 0.1f);
            animator.SetFloat("Speed", 0); // ปิด Animation วิ่งปกติ
        }

        // **2. STAND UP TRIGGER (เมื่อปล่อยปุ่ม)**
        // 🎯 ใช้ Input.GetKeyUp เพื่อเป็นตัวสั่งให้ลุกขึ้นโดยเฉพาะ 
        // ทำให้สถานะไม่แกว่งแม้ IsGrounded() จะผิดพลาดชั่วคราว
        if (Input.GetKeyUp(KeyCode.LeftShift) && isCrouching)
        {
            // ลุกขึ้น
            isCrouching = false;

            // คืนค่า Collider
            boxCollider.size = new Vector2(boxCollider.size.x, originalHeight);
            boxCollider.offset = originalOffset;

            // ปิด Animation Crouch
            animator.SetBool("isCrouchWalking", false);
            animator.SetBool("isCrouchIdle", false);
        }

        // **3. FORCED STAND UP (เมื่อตกจากขอบขณะย่อตัว)**
        // 🎯 เงื่อนไขนี้จะบังคับให้ลุกขึ้นหากไม่อยู่บนพื้น (IsGrounded() เป็น false) และไม่ได้กด Shift ค้าง
        if (isCrouching && !IsGrounded() && !Input.GetKey(KeyCode.LeftShift))
        {
            isCrouching = false;

            // คืนค่า Collider
            boxCollider.size = new Vector2(boxCollider.size.x, originalHeight);
            boxCollider.offset = originalOffset;

            // ปิด Animation Crouch
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