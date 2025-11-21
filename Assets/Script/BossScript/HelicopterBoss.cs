using UnityEngine;
using System.Collections;

public class HelicopterBoss : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform topPoint;
    public Transform bottomPoint;

    [Header("Intro Settings")]
    public Transform entryPoint; // จุดที่บอสเริ่มบินเข้ามา (นอกจอ)
    public float entrySpeed = 5f; // ความเร็วตอนบินเข้าฉาก

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletFireRate = 0.5f;

    public GameObject missilePrefab;
    public Transform missilePoint;
    public float missileFireRate = 1.2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip machineGunSound;
    public AudioClip missileSound;

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

    // เช็คว่าเริ่มสู้หรือยัง
    private bool isBattleActive = false;

    public System.Action onBossDefeated;

    void Start()
    {
        currentHP = maxHP;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // ** ปิดการทำงานเริ่มต้น **
        // ย้ายบอสไปจุด Entry Point (นอกจอ) ก่อน
        if (entryPoint != null)
            transform.position = entryPoint.position;
        else if (topPoint != null)
            transform.position = topPoint.position;
    }

    void Update()
    {
        // ถ้ายังไม่เริ่มสู้ (กำลังบินเข้าฉาก) ห้ามขยับ Phase หรือยิง
        if (!isBattleActive) return;

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= phaseDuration)
        {
            phaseTimer = 0f;
            SwitchPhase();
        }

        Move();
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย BossActivator
    public void StartBossSequence()
    {
        StartCoroutine(IntroMoveRoutine());
    }

    IEnumerator IntroMoveRoutine()
    {
        // บินจาก EntryPoint ไปยัง TopPoint
        if (entryPoint != null && topPoint != null)
        {
            transform.position = entryPoint.position;

            while (Vector3.Distance(transform.position, topPoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, topPoint.position, entrySpeed * Time.deltaTime);
                yield return null;
            }
        }

        // เมื่อถึงที่แล้ว เริ่มการต่อสู้
        isBattleActive = true;

        // เริ่มยิง Missile เป็นอย่างแรก (ตาม Logic เดิม)
        if (missilePoint != null && missilePrefab != null)
            attackRoutine = StartCoroutine(MissileAttack());

        Debug.Log("Boss Intro Finished! Fight Started.");
    }

    void Move()
    {
        if (topPoint == null || bottomPoint == null) return;

        Vector3 target = inTopPhase ? topPoint.position : bottomPoint.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    // ... (ส่วนอื่นๆ เหมือนเดิม: SwitchPhase, Attacks, TakeDamage, HitFlash, OnCollision) ...

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
                Instantiate(missilePrefab, missilePoint.position, Quaternion.identity)
                    .GetComponent<MissileBoss>()?.SetDirection(playerPos);

                PlaySound(missileSound, 0.8f);
            }
            yield return new WaitForSeconds(missileFireRate);
        }
    }

    IEnumerator MachineGunAttack()
    {
        while (!inTopPhase)
        {
            ShootBulletLeft();
            PlaySound(machineGunSound, 0.7f);
            yield return new WaitForSeconds(bulletFireRate);
        }
    }

    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
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
        // ถ้ายังไม่เริ่มสู้ ห้ามรับดาเมจ (กันผู้เล่นโกงยิงตอน Intro)
        if (!isBattleActive) return;

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

    IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
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