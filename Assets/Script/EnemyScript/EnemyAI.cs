using UnityEngine;
using System.Collections;

[System.Serializable]
public class LootItem
{
    public GameObject itemPrefab;
    public float dropRate; // แค่ float ธรรมดา ไม่ต้อง Range
}

public class EnemyAI : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float moveSpeed = 3f;
    public float detectionRange = 6f;
    public float stopDistance = 3f;

    [Header("Combat")]
    public float fireRate = 1f;
    public float firstShotDelay = 5f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject muzzleFlashPrefab;

    [Header("Audio")]
    public AudioClip shootSound;
    private AudioSource audioSource;

    [Header("Stats")]
    public int maxHP = 3;
    private int currentHP;

    [Header("Hit Feedback")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.2f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Loot Table")]
    public LootItem[] lootTable;

    private Transform targetPlayer;
    private float fireCooldown;
    private bool hasStartedShooting = false;
    private float detectTimer = 0f;
    private Animator animator;
    private bool isDeadAndDropped = false;

    // =============================
    // ป้องกัน Unity รีเซ็ตค่า dropRate
    // =============================
    void OnValidate()
    {
        if (lootTable == null) return;

        foreach (var loot in lootTable)
        {
            if (loot.dropRate <= 0f)
                loot.dropRate = 1f; // ถ้า dropRate <= 0 ให้กำหนดค่าเริ่มต้น 1
        }
    }

    void Awake()
    {
        currentHP = maxHP;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        FindClosestPlayer();
        UpdateState();
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

            Vector2 lookDir = (targetPlayer.position - transform.position).normalized;
            spriteRenderer.flipX = (lookDir.x < 0);

            if (firePoint != null)
            {
                Vector3 localPos = firePoint.localPosition;
                localPos.x = spriteRenderer.flipX ? -Mathf.Abs(localPos.x) : Mathf.Abs(localPos.x);
                firePoint.localPosition = localPos;
            }

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

    bool CanSeePlayer()
    {
        if (targetPlayer == null) return false;

        Vector2 origin = transform.position;
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer <= detectionRange && Mathf.Abs(targetPlayer.position.y - transform.position.y) <= 1.5f)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, detectionRange, LayerMask.GetMask("Default", "Player"));
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.DrawLine(origin, hit.point, Color.red);
                return true;
            }
        }

        Debug.DrawLine(origin, origin + direction * detectionRange, Color.gray);
        return false;
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
                Destroy(Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation), 0.1f);

            if (shootSound != null)
                audioSource.PlayOneShot(shootSound);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            if (isDeadAndDropped) return; // ป้องกันการเรียกซ้ำ
            isDeadAndDropped = true; // ตั้งค่า Flag

            DropLoot();
            Destroy(gameObject, 0.1f);
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

    // ใน EnemyAI.cs

    void DropLoot()
    {
        if (lootTable == null || lootTable.Length == 0) return;

        // 1. คำนวณน้ำหนักรวมของโอกาส Drop ทั้งหมด
        float totalWeight = 0f;
        foreach (var loot in lootTable)
            totalWeight += loot.dropRate;

        if (totalWeight <= 0f) return;

        // 2. สุ่มตัวเลข (Roll) ระหว่าง 0 ถึง Total Weight
        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float current = 0f;

        // 3. วนลูปเพื่อหา Item ที่ Roll ไปตกอยู่
        foreach (var loot in lootTable)
        {
            current += loot.dropRate; // เพิ่มน้ำหนักสะสม

            // ถ้าตัวเลข Roll น้อยกว่าหรือเท่ากับน้ำหนักสะสมปัจจุบัน
            if (roll < current)
            {
                // Drop Item นั้น
                Instantiate(loot.itemPrefab, transform.position, Quaternion.identity);

                // ** ⬅️ นี่คือหัวใจสำคัญ: หยุดการทำงานทันทีเพื่อไม่ให้ Drop ชิ้นอื่น **
                return;
            }
        }
    }
}
