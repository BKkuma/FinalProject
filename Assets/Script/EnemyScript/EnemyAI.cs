using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float moveSpeed = 3f;
    [Tooltip("ระยะที่ศัตรูสามารถตรวจเจอผู้เล่นได้")]
    public float detectionRange = 6f; // 🔹 ลดจาก 10f → 6f
    public float stopDistance = 3f;

    [Header("Combat")]
    public float fireRate = 1f;
    public float firstShotDelay = 5f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject muzzleFlashPrefab;

    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Hit Feedback")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.2f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private Transform targetPlayer;
    private float fireCooldown;
    private bool hasStartedShooting = false;
    private float detectTimer = 0f;

    private Animator animator;

    void Awake()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        FindClosestPlayer();
        UpdateState();

        // 🔹 เรียกเพื่อให้เห็นเส้นสายตาใน Scene View
        CanSeePlayer();
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player.transform;
            }
        }

        targetPlayer = closestPlayer;
    }

    void UpdateState()
    {
        if (targetPlayer == null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isShooting", false);
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance < detectionRange)
        {
            // เดินเข้า player ถ้ายังไกลกว่า stopDistance
            if (distance > stopDistance)
            {
                Vector2 direction = (targetPlayer.position - transform.position).normalized;
                transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
                animator.SetBool("isWalking", true);
                animator.SetBool("isShooting", false);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }

            // หัน sprite ตามทิศทาง player
            Vector2 lookDir = (targetPlayer.position - transform.position).normalized;
            spriteRenderer.flipX = (lookDir.x < 0);

            // ปรับตำแหน่ง firePoint ตามการหัน
            if (firePoint != null)
            {
                Vector3 localPos = firePoint.localPosition;
                localPos.x = spriteRenderer.flipX ? -Mathf.Abs(localPos.x) : Mathf.Abs(localPos.x);
                firePoint.localPosition = localPos;
            }

            // 🔹 ยิงเฉพาะเมื่อ “เห็นจริง” ผ่านเส้นตรง
            if (CanSeePlayer())
            {
                if (!hasStartedShooting)
                {
                    detectTimer += Time.deltaTime;
                    if (detectTimer >= firstShotDelay)
                    {
                        Shoot();
                        hasStartedShooting = true;
                        fireCooldown = 1f / fireRate;
                        animator.SetBool("isShooting", true);
                    }
                }
                else
                {
                    fireCooldown -= Time.deltaTime;
                    if (fireCooldown <= 0f)
                    {
                        Shoot();
                        fireCooldown = 1f / fireRate;
                        animator.SetBool("isShooting", true);
                    }
                }
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isShooting", false);
            detectTimer = 0f;
            hasStartedShooting = false;
        }
    }

    // ✅ ใช้เส้น Raycast ตรวจสายตา + Debug แสดงเส้น
    bool CanSeePlayer()
    {
        if (targetPlayer == null) return false;

        Vector2 origin = transform.position;
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        Color debugColor = Color.gray;

        if (distanceToPlayer <= detectionRange && Mathf.Abs(targetPlayer.position.y - transform.position.y) <= 1.5f)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectionRange, LayerMask.GetMask("Default", "Player"));
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    debugColor = Color.red; // 🔴 เห็นผู้เล่น
                    Debug.DrawLine(origin, hit.point, debugColor);
                    return true;
                }
                else
                {
                    debugColor = Color.yellow; // 🟡 มีสิ่งกีดขวาง
                    Debug.DrawLine(origin, hit.point, debugColor);
                    return false;
                }
            }
        }

        Debug.DrawLine(origin, origin + direction * detectionRange, debugColor);
        return false;
    }

    // ✅ วาด Gizmo แสดงระยะมองเห็น
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // วงระยะตรวจจับ

        // วาดเส้นสายตาแนวนอน (ซ้าย-ขวา)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position - Vector3.right * detectionRange);
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null && targetPlayer != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (targetPlayer.position - firePoint.position).normalized;
                rb.velocity = dir * 7f;
            }

            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        StartCoroutine(HitFlash());
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }
}
