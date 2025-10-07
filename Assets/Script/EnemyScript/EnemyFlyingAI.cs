using UnityEngine;
using System.Collections;

public class EnemyFlyingAI : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float moveSpeed = 3f;
    public float detectionRange = 12f;
    public float orbitDistance = 4f;
    public float orbitSpeed = 50f;

    [Header("Combat")]
    public float fireRate = 1f;
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
    private Rigidbody2D rb;

    // ✅ ตัวแปรใหม่
    private bool canShoot = false;
    private bool startedShootDelay = false;

    void Awake()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
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
                Vector2 direction = (targetPlayer.position - transform.position).normalized;

                // เข้าใกล้ player จนถึงระยะ orbitDistance
                if (distance > orbitDistance)
                {
                    rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
                }
                else
                {
                    // โคจรรอบผู้เล่น
                    rb.MovePosition(
                        rb.position + (Vector2)(Quaternion.Euler(0, 0, orbitSpeed * Time.deltaTime)
                        * (transform.position - targetPlayer.position).normalized) * moveSpeed * Time.deltaTime
                    );
                }

                // Flip Sprite
                spriteRenderer.flipX = direction.x >= 0;

                // ✅ เริ่มนับถอยหลังก่อนยิง
                if (!startedShootDelay)
                {
                    startedShootDelay = true;
                    StartCoroutine(StartShootingAfterDelay(2f));
                }

                // ✅ ยิงเฉพาะเมื่อ canShoot = true
                if (canShoot)
                {
                    fireCooldown -= Time.deltaTime;
                    if (fireCooldown <= 0f)
                    {
                        Shoot();
                        fireCooldown = 1f / fireRate;
                    }
                }
            }
        }
    }

    IEnumerator StartShootingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canShoot = true;
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

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, orbitDistance);
    }
}
