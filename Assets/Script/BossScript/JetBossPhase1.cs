using UnityEngine;
using System.Collections;

public class JetBossPhase1 : MonoBehaviour
{
    [Header("Movement & Arena")]
    public float moveSpeed = 5f;
    public Transform[] movePoints; // จุดที่จะบินวนรอบ
    private int currentPointIndex = 0;
    private bool isMoving = false;
    public float waitTimeAtPoint = 1.5f; // เวลาให้บอสหยุดก่อนเลือกจุดต่อไป

    [Header("Combat")]
    public GameObject bulletPrefab;
    public float bulletFireRate = 0.5f;
    public Vector2[] shootDirections = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    }; // ยิง Multi-Directional (0°, 90°, 180°, 270°)
    public Transform firePoint;

    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;
    private Color originalColor;
    private Coroutine hitFlashRoutine;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;

    // Callback แจ้ง ArenaController ว่าบอสตาย
    public System.Action onBossDefeated;

    private Coroutine attackRoutine;

    void Start()
    {
        currentHP = maxHP;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        isMoving = true;
        StartCoroutine(MoveAndAttackRoutine());
    }

    IEnumerator MoveAndAttackRoutine()
    {
        while (true)
        {
            if (movePoints.Length == 0)
                yield return null;

            Transform targetPoint = movePoints[currentPointIndex];

            // เคลื่อนที่ไปยังจุด
            while (Vector3.Distance(transform.position, targetPoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // อยู่จุดนั้นก่อน
            yield return new WaitForSeconds(waitTimeAtPoint);

            // เริ่มยิง
            attackRoutine = StartCoroutine(ShootMultiDirectional());

            // เลือกจุดถัดไปแบบสุ่ม (ไม่ซ้ำ)
            currentPointIndex = GetNextPointIndex();

            // รอจนหยุดยิง ก่อนเคลื่อนต่อ
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }
        }
    }

    int GetNextPointIndex()
    {
        if (movePoints.Length <= 1) return 0;

        int nextIndex = Random.Range(0, movePoints.Length);
        while (nextIndex == currentPointIndex)
        {
            nextIndex = Random.Range(0, movePoints.Length);
        }
        return nextIndex;
    }

    IEnumerator ShootMultiDirectional()
    {
        while (true)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                foreach (Vector2 dir in shootDirections)
                {
                    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.velocity = dir.normalized * 7f; // ปรับความเร็วกระสุนได้
                }

                if (audioSource != null && shootSound != null)
                    audioSource.PlayOneShot(shootSound);
            }

            yield return new WaitForSeconds(bulletFireRate);
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            onBossDefeated?.Invoke();
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
        hitFlashRoutine = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Bullet playerBullet = collision.gameObject.GetComponent<Bullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
