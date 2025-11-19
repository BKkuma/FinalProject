// JetBossPhase1.cs
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
    private bool isDead = false;

    [Header("Escape / Phase2 Transition")]
    public Transform escapePoint;
    public float escapeSpeed = 8f;
    public float flyUpDistance = 10f;
    public System.Action onPhase1Finished;

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

    void Start()
    {
        currentHP = maxHP;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        currentSidePoints = movingRight ? leftPoints.ToList() : rightPoints.ToList();
        currentPointIndex = 0;
    }

    public void StartBossBehavior()
    {
        if ((leftPoints != null && leftPoints.Length > 0) || (rightPoints != null && rightPoints.Length > 0))
        {
            StartCoroutine(BossBehavior());
        }
    }

    IEnumerator BossBehavior()
    {
        while (!isDead)
        {
            if (currentSidePoints == null || currentSidePoints.Count == 0)
            {
                yield return null;
                continue;
            }

            if (currentPointIndex >= currentSidePoints.Count)
            {
                movingRight = !movingRight;
                currentSidePoints = movingRight ? leftPoints.ToList() : rightPoints.ToList();
                currentPointIndex = 0;
                if (currentSidePoints.Count == 0) yield return null;
            }

            Transform target = currentSidePoints[currentPointIndex];
            if (target == null) { currentPointIndex++; continue; }

            while (Vector3.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(waitTimeAtPoint);

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                int attackType = Random.Range(0, 2);
                if (attackType == 0)
                    yield return StartCoroutine(FanShotAtPlayer(player.transform.position));
                else
                    yield return StartCoroutine(HomingShot(player.transform.position, 3));
            }

            currentPointIndex++;
        }
    }

    IEnumerator FanShotAtPlayer(Vector3 playerPos)
    {
        if (bulletPrefab == null || firePoint == null) yield break;

        Vector2 directionToPlayer = (playerPos - firePoint.position).normalized;
        float centerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        float startAngle = centerAngle - fanAngle / 2f;
        float angleStep = fanAngle / (fanBulletCount - 1);

        for (int i = 0; i < fanBulletCount; i++)
        {
            float totalAngle = startAngle + angleStep * i;
            Vector2 dir = new Vector2(Mathf.Cos(totalAngle * Mathf.Deg2Rad), Mathf.Sin(totalAngle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, totalAngle));
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * homingSpeed;
        }

        if (audioSource != null && shootSound != null) audioSource.PlayOneShot(shootSound);
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
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * homingSpeed;

            if (audioSource != null && shootSound != null) audioSource.PlayOneShot(shootSound);
            yield return new WaitForSeconds(bulletFireRate);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        if (hitFlashRoutine != null) StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(BossEscape());
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

    IEnumerator BossEscape()
    {
        if (escapePoint != null)
        {
            while (Vector3.Distance(transform.position, escapePoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, escapePoint.position, escapeSpeed * Time.deltaTime);
                yield return null;
            }
        }

        yield return new WaitForSeconds(Random.Range(1f, 2f));

        Vector3 targetUp = transform.position + Vector3.up * flyUpDistance;
        while (Vector3.Distance(transform.position, targetUp) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetUp, escapeSpeed * Time.deltaTime);
            yield return null;
        }

        if (onPhase1Finished != null)
        {
            onPhase1Finished.Invoke();
            onPhase1Finished = null;
        }

        Destroy(gameObject);
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
