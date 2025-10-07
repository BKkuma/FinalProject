using UnityEngine;
using System.Collections;

public class HelicopterBoss : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform topPoint;
    public Transform bottomPoint;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletFireRate = 0.5f;

    public GameObject missilePrefab;
    public Transform missilePoint;
    public float missileFireRate = 1.2f;

    [Header("Attack Cycle")]
    public float phaseDuration = 10f;
    private bool inTopPhase = true;
    private float phaseTimer = 0f;

    [Header("Stats")]
    public int maxHP = 50;
    private int currentHP;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;

    private Coroutine attackRoutine;
    private Coroutine hitFlashRoutine;
    private Color originalColor;

    public System.Action onBossDefeated; // Event ให้เรียกตอน Boss ตาย

    void Start()
    {
        currentHP = maxHP;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color; // เก็บสีเดิมตั้งแต่เริ่มเกม

        if (topPoint != null)
            transform.position = topPoint.position;

        if (missilePoint != null && missilePrefab != null)
            attackRoutine = StartCoroutine(MissileAttack());
    }

    void Update()
    {
        phaseTimer += Time.deltaTime;
        if (phaseTimer >= phaseDuration)
        {
            phaseTimer = 0f;
            SwitchPhase();
        }

        Move();
    }

    void Move()
    {
        if (topPoint == null || bottomPoint == null) return;

        Vector3 target = inTopPhase ? topPoint.position : bottomPoint.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    void SwitchPhase()
    {
        inTopPhase = !inTopPhase;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        if (inTopPhase)
        {
            if (missilePoint != null && missilePrefab != null)
                attackRoutine = StartCoroutine(MissileAttack());
        }
        else
        {
            attackRoutine = StartCoroutine(MachineGunAttack());
        }
    }

    IEnumerator MissileAttack()
    {
        while (inTopPhase)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && missilePoint != null && missilePrefab != null)
            {
                Vector3 playerPos = player.transform.position;
                GameObject missile = Instantiate(missilePrefab, missilePoint.position, Quaternion.identity);
                missile.GetComponent<MissileBoss>()?.SetDirection(playerPos);
            }
            yield return new WaitForSeconds(missileFireRate);
        }
    }

    IEnumerator MachineGunAttack()
    {
        while (!inTopPhase)
        {
            ShootBulletLeft();
            yield return new WaitForSeconds(bulletFireRate);
        }
    }

    void ShootBulletLeft()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            BulletBoss bulletScript = bullet.GetComponent<BulletBoss>();
            if (bulletScript != null)
                bulletScript.direction = Vector2.left;

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null && bulletScript != null)
                rb.velocity = bulletScript.direction * bulletScript.speed;
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        // ถ้า Coroutine เก่าอยู่ ให้หยุดก่อน
        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            if (UIManager.instance != null)
                UIManager.instance.ShowWinUI(); // <<--- เรียกจบเกมตรงนี้

            Destroy(gameObject);
        }
    }


    IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor; // กลับสีเดิม
            hitFlashRoutine = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
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
