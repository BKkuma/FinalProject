using UnityEngine;
using System.Collections;

public class EnemyFlyingAI : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float moveSpeed = 3f;
    public float detectionRange = 12f;
    public float orbitDistance = 4f;

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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSFX;
    public AudioClip hitSFX;
    public AudioClip deathSFX;

    private Transform targetPlayer;
    private float fireCooldown;
    private Rigidbody2D rb;
    private bool canShoot = false;
    private bool startedShootDelay = false;

    void Awake()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        FindClosestPlayer();

        if (targetPlayer == null || rb == null) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        if (distance > orbitDistance)
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
        else
        {
            float hoverSpeed = 2f;
            Vector2 hoverOffset = new Vector2(Mathf.Sin(Time.time * hoverSpeed), Mathf.Cos(Time.time * hoverSpeed * 0.5f)) * 0.3f;
            rb.MovePosition(rb.position + hoverOffset * Time.deltaTime);
        }

        if (distance <= detectionRange)
        {
            if (!startedShootDelay)
            {
                startedShootDelay = true;
                StartCoroutine(StartShootingAfterDelay(2f));
            }

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

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x >= 0;
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
        Transform closest = null;

        foreach (GameObject player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = player.transform;
            }
        }

        targetPlayer = closest;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || targetPlayer == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
            rbBullet.velocity = (targetPlayer.position - firePoint.position).normalized * 7f;

        if (shootSFX != null)
            audioSource.PlayOneShot(shootSFX);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        StartCoroutine(HitFlash());

        if (hitSFX != null)
            audioSource.PlayOneShot(hitSFX);

        if (currentHP <= 0)
        {
            // ใช้ระบบดรอปใหม่แทน
            GetComponent<EnemyDrop>()?.Drop();

            if (deathSFX != null)
                audioSource.PlayOneShot(deathSFX);

            Destroy(gameObject, 0.1f);
        }
    }

    IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }
}
