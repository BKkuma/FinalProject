using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JetBossPhase1 : MonoBehaviour
{
    [Header("Movement & Arena")]
    public float moveSpeed = 5f;
    public Transform[] leftPoints;
    public Transform[] rightPoints;
    public float waitTimeAtPoint = 1.5f;

    private List<Transform> currentSidePoints;
    private int currentPointIndex = 0;
    private bool movingRight = true;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletFireRate = 0.4f;
    public float fanAngle = 45f;
    public int fanBulletCount = 5;
    public float homingSpeed = 7f;

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

    public System.Action onBossDefeated;

    void Start()
    {
        currentHP = maxHP;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        currentSidePoints = movingRight ? leftPoints.ToList() : rightPoints.ToList();
        currentPointIndex = 0;

        StartCoroutine(BossBehavior());
    }

    IEnumerator BossBehavior()
    {
        while (true)
        {
            if (currentSidePoints.Count == 0) yield return null;

            Transform target = currentSidePoints[currentPointIndex];

            // ---- MOVE ----
            while (Vector3.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // ---- WAIT ----
            yield return new WaitForSeconds(waitTimeAtPoint);

            // ---- SHOOT ----
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                int attackType = Random.Range(0, 2); // 0 = Fan Shot, 1 = Homing Shot
                if (attackType == 0)
                    yield return StartCoroutine(FanShotAtPlayer(player.transform.position));
                else
                    yield return StartCoroutine(HomingShot(player.transform.position, 3));

            }

            // ---- PICK NEXT POINT ----
            currentPointIndex++;
            if (currentPointIndex >= currentSidePoints.Count)
            {
                movingRight = !movingRight;
                currentSidePoints = movingRight ? leftPoints.ToList() : rightPoints.ToList();
                currentPointIndex = 0;
            }
        }
    }

    IEnumerator FanShotAtPlayer(Vector3 playerPos)
    {
        if (bulletPrefab == null || firePoint == null) yield break;

        Vector2 directionToPlayer = (playerPos - firePoint.position).normalized;
        float centerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        float startAngle = centerAngle - fanAngle / 2f;
        float angleStep = fanAngle / (fanBulletCount - 1);

        float spriteOffset = 0f; // สำหรับกระสุนแนวนอนหันขวา

        for (int i = 0; i < fanBulletCount; i++)
        {
            float totalAngle = startAngle + angleStep * i + spriteOffset;
            Vector2 dir = new Vector2(Mathf.Cos(totalAngle * Mathf.Deg2Rad), Mathf.Sin(totalAngle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, totalAngle));
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * homingSpeed;
        }

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        yield return new WaitForSeconds(bulletFireRate);
    }

    IEnumerator HomingShot(Vector3 playerPos, int shots = 3)
    {
        if (bulletPrefab == null || firePoint == null) yield break;

        for (int i = 0; i < shots; i++)
        {
            Vector2 dir = (playerPos - firePoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float spriteOffset = 0f; // สำหรับกระสุนแนวนอนหันขวา
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle + spriteOffset);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = dir * homingSpeed;

            if (audioSource != null && shootSound != null)
                audioSource.PlayOneShot(shootSound);

            yield return new WaitForSeconds(bulletFireRate); // หน่วงระหว่างนัด
        }
    }


    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            onBossDefeated?.Invoke();
            Destroy(gameObject);
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
