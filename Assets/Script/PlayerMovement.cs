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
    // 🎯 ตัวแปรสถานะเพื่อตรวจสอบการเล็งในแนวตั้ง (สำหรับควบคุม Animation Priority)
    private bool isAimingVertical = false;

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

        // เก็บค่าเริ่มต้นของ Box Collider ไว้
        originalHeight = boxCollider.size.y;
        originalOffset = boxCollider.offset;
    }

    void Update()
    {
        // 🎯 1. ตรวจสอบการเล็งก่อนเสมอ เพื่อให้สถานะ isAimingVertical ถูกตั้งค่าในเฟรมปัจจุบัน
        AimDirection();

        // 🎯 2. ตรวจสอบการเคลื่อนที่ โดยใช้สถานะการเล็งที่เพิ่งตั้งค่าไป
        Move();

        Jump();
        Crouch();
    }

    // ----------------------------------------------------
    // Movement & Actions
    // ----------------------------------------------------

    void Move()
    {
        float move = 0;

        // 1. ตรวจสอบการกด D/A เพื่อกำหนดทิศทางการเคลื่อนที่
        if (Input.GetKey(KeyCode.A))
        {
            move = -1;
            // 🎯 กำหนดทิศทางการหันหน้า/ยิงแนวนอน เมื่อไม่มีการเล็งแนวตั้ง
            if (!isAimingVertical)
            {
                shootDirection = Vector2.left;
                spriteRenderer.flipX = true;
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            move = 1;
            // 🎯 กำหนดทิศทางการหันหน้า/ยิงแนวนอน เมื่อไม่มีการเล็งแนวตั้ง
            if (!isAimingVertical)
            {
                shootDirection = Vector2.right;
                spriteRenderer.flipX = false;
            }
        }

        // Animation และการเคลื่อนที่
        if (!isCrouching)
        {
            if (isAimingVertical)
            {
                // 🎯 ขณะเล็งแนวตั้ง: อนุญาตให้เดิน (A/D) ได้ แต่ให้ Animation เล็งแนวตั้งมี Priority
                rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
                animator.SetFloat("Speed", 0); // 🎯 ตั้งค่า Speed เป็น 0 เพื่อหยุด Animation วิ่ง
            }
            else
            {
                // การเคลื่อนที่ปกติ
                rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
                animator.SetFloat("Speed", Mathf.Abs(move));
            }
        }
        else // เดินย่อตัว
        {
            rb.velocity = new Vector2(move * (moveSpeed * 0.5f), rb.velocity.y);
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
        isAimingVertical = false; // รีเซ็ตทุกเฟรม

        if (Input.GetKey(KeyCode.W))
        {
            // W มี Priority สูงสุด: หันไปทาง W
            shootDirection = Vector2.up;
            animator.SetBool("isAimingUp", true);
            animator.SetBool("isAimingDown", false);
            isAimingVertical = true;
        }
        else if (Input.GetKey(KeyCode.S) && !isCrouching)
        {
            // S มี Priority รองลงมา: หันไปทาง S
            shootDirection = Vector2.down;
            animator.SetBool("isAimingDown", true);
            animator.SetBool("isAimingUp", false);
            isAimingVertical = true;
        }
        else
        {
            // ถ้าไม่มีการกด W/S: รีเซ็ต Vertical Aims
            animator.SetBool("isAimingUp", false);
            animator.SetBool("isAimingDown", false);

            // ❌ ไม่ต้องตั้งค่า shootDirection ตรงนี้ ให้ฟังก์ชัน Move() เป็นผู้จัดการทิศทางแนวนอน
        }
    }

    void Crouch()
    {
        // **1. CROUCH DOWN / STAY CROUCHED**
        if (Input.GetKey(KeyCode.LeftShift) && IsGrounded())
        {
            if (!isCrouching) // ทำการปรับขนาด Collider เพียงครั้งเดียวตอนเข้าสู่สถานะย่อตัว
            {
                isCrouching = true;

                // ปรับขนาด Box Collider (50% ของความสูงเดิม)
                float crouchHeight = originalHeight * 0.5f;
                boxCollider.size = new Vector2(boxCollider.size.x, crouchHeight);
                boxCollider.offset = new Vector2(originalOffset.x, originalOffset.y - (originalHeight - crouchHeight) / 2f);
            }

            // อัปเดต Animation ทุกเฟรมขณะย่อ
            animator.SetBool("isCrouchIdle", Mathf.Abs(rb.velocity.x) < 0.1f);
            animator.SetBool("isCrouchWalking", Mathf.Abs(rb.velocity.x) >= 0.1f);
            animator.SetFloat("Speed", 0);
        }

        // **2. STAND UP TRIGGER (เมื่อปล่อยปุ่ม)**
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