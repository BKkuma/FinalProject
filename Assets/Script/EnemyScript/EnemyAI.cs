using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 3f;

    [Header("Combat")]
    public float fireRate = 1f;
    public float firstShotDelay = 5f; // เวลารอก่อนยิงครั้งแรก
    public GameObject bulletPrefab;
    public Transform firePoint;

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
    private bool hasStartedShooting = false; // เช็คว่ายิงครั้งแรกหรือยัง
    private float detectTimer = 0f;          // ตัวจับเวลาเมื่อเจอ player

    void Awake()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        FindClosestPlayer();

        if (targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.position);

            if (distance < detectionRange)
            {
                // เดินเข้า player ถ้ายังไกลกว่า stopDistance
                if (distance > stopDistance)
                {
                    Vector2 direction = (targetPlayer.position - transform.position).normalized;
                    transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
                }

                // หัน sprite ตามทิศทาง player
                Vector2 lookDir = (targetPlayer.position - transform.position).normalized;
                spriteRenderer.flipX = (lookDir.x >= 0);

                // ถ้ายังไม่เคยยิง → เริ่มจับเวลา
                if (!hasStartedShooting)
                {
                    detectTimer += Time.deltaTime;
                    if (detectTimer >= firstShotDelay)
                    {
                        Shoot();
                        hasStartedShooting = true;
                        fireCooldown = 1f / fireRate;
                    }
                }
                else
                {
                    // ยิงปกติหลังจากยิงครั้งแรกแล้ว
                    fireCooldown -= Time.deltaTime;
                    if (fireCooldown <= 0f)
                    {
                        Shoot();
                        fireCooldown = 1f / fireRate;
                    }
                }
            }
            else
            {
                // ออกนอก detectionRange → reset timer
                detectTimer = 0f;
                hasStartedShooting = false;
            }
        }
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

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null && targetPlayer != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            BulletEnemy bulletScript = bullet.GetComponent<BulletEnemy>();
            if (bulletScript != null)
            {
                bulletScript.ShootTowards(targetPlayer.position);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
