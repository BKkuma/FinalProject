using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform firePoint;
    public Transform crouchFirePoint;  // จุดยิงตอน Crouch

    private Rigidbody2D rb;
    private Vector2 shootDirection = Vector2.right;
    private int jumpCount = 0;
    public int maxJumps = 2;

    private bool isCrouching = false;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    private float originalRadius;
    private Vector2 originalOffset;
    private Animator animator;

    public bool IsCrouching => isCrouching;
    public Vector2 ShootDirection => shootDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();

        originalRadius = circleCollider.radius;
        originalOffset = circleCollider.offset;
    }

    void Update()
    {
        Move();
        Jump();
        AimDirection();
        Crouch();
    }

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

        rb.velocity = isCrouching ? new Vector2(0, rb.velocity.y) : new Vector2(move * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(move));
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.J) && jumpCount < maxJumps && !isCrouching)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;

            animator.SetBool("isJumping", true); // ✅ เปิด animation กระโดด
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
            // ไม่หันขึ้นหรือลง → ปิด animation
            animator.SetBool("isAimingUp", false);
            animator.SetBool("isAimingDown", false);
        }
    }

    void Crouch()
    {
        if (Input.GetKey(KeyCode.LeftShift) && rb.velocity.y == 0)
        {
            isCrouching = true;
            circleCollider.radius = originalRadius * 0.5f;
            circleCollider.offset = new Vector2(originalOffset.x, originalOffset.y - (originalRadius * 0.5f));
            shootDirection = Vector2.right; // ยิงตรงตอนย่อ
        }
        else
        {
            isCrouching = false;
            circleCollider.radius = originalRadius;
            circleCollider.offset = originalOffset;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal == Vector2.up)
        {
            jumpCount = 0;
            animator.SetBool("isJumping", false); // ✅ ปิด animation กระโดด
        }

        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }
}
