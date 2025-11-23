using UnityEngine;
using System.Collections;
using System;

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

    // ⭐ NEW: ความเร็วของกระสุนปืนกล (ต้องตั้งค่าใน Inspector)
    [Header("Bullet Settings")]
    public float machineGunBulletSpeed = 10f;

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

    [Header("Explosion Settings")]
    public GameObject explosionPrefab; // ลาก Prefab ระเบิดมาใส่ตรงนี้
    public int numExplosions = 5;      // จำนวนครั้งที่ระเบิดจะปรากฏ
    public float explosionInterval = 0.2f; // ระยะเวลาระหว่างระเบิดแต่ละลูก

    private Coroutine attackRoutine;
    private Coroutine hitFlashRoutine;
    private Color originalColor;

    private bool isDefeated = false;
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
        if (entryPoint != null)
            transform.position = entryPoint.position;
        else if (topPoint != null)
            transform.position = topPoint.position;
    }

    void Update()
    {
        // ถ้ายังไม่เริ่มสู้ หรือบอสตายแล้ว ให้หยุดการทำงาน
        if (!isBattleActive || isDefeated) return;

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
                if (isDefeated) yield break; // ป้องกันการทำงานต่อถ้าตาย
            }
        }

        // เมื่อถึงที่แล้ว เริ่มการต่อสู้
        isBattleActive = true;

        // เริ่มยิง Missile เป็นอย่างแรก
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
        while (inTopPhase && !isDefeated)
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
        while (!inTopPhase && !isDefeated)
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

    // ⭐ MODIFIED: ปรับให้กระสุนพุ่งไปทาง Vector2.left โดยเรียก SetDirection()
    void ShootBulletLeft()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            BulletBoss bulletScript = bullet.GetComponent<BulletBoss>();
            if (bulletScript != null)
            {
                // สั่งให้กระสุนพุ่งไปทางซ้าย (Vector2.left)
                bulletScript.SetDirection(Vector2.left);
            }
            else
            {
                Debug.LogError("Bullet Prefab is missing the BulletBoss script!");
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        // ถ้ายังไม่เริ่มสู้ หรือบอสตายแล้ว ห้ามรับดาเมจ
        if (!isBattleActive || isDefeated) return;

        currentHP -= dmg;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0 && !isDefeated)
        {
            isDefeated = true;
            StopAllCoroutines();
            StartCoroutine(DeathSequence()); // ⭐ เรียก Coroutine การระเบิด
        }
    }

    IEnumerator DeathSequence()
    {
        // 1. ซ่อน Sprite Renderer และ Collider
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        // 2. สร้างเอฟเฟกต์ระเบิดซ้ำๆ
        if (explosionPrefab != null)
        {
            for (int i = 0; i < numExplosions; i++)
            {
                // สร้างระเบิดในตำแหน่งสุ่มรอบๆ บอส
                Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0);
                Instantiate(explosionPrefab, transform.position + randomOffset, Quaternion.identity);
                yield return new WaitForSeconds(explosionInterval);
            }
        }

        // 3. แจ้งเตือนสคริปต์อื่น ๆ ว่าบอสตายแล้ว
        onBossDefeated?.Invoke();


        // 4. ทำลาย GameObject หลักของบอส
        Destroy(gameObject);
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
        if (isDefeated) return;

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