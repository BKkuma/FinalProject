using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    // 🔴 ลบ: maxHealth และ currentHealth ออกไป

    [Header("UI")]
    public GameObject gameOverUI;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnInvincibleTime = 3f; // ระยะเวลาอมตะหลังเกิด
    public int autoRespawnLives = 3;

    [Header("Laser Landing Effect")]
    [Tooltip("Prefab ของลำแสงที่ Instatiate ออกมา")]
    public GameObject laserLandingPrefab;

    [Tooltip("ปรับตำแหน่ง Effect ลำแสง (X, Y, Z) เช่น Y=1 เพื่อให้สูงขึ้น")]
    public Vector3 laserSpawnOffset;

    [Tooltip("ระยะเวลาตั้งแต่ลำแสงเริ่มจนกระทั่งตัวละครโผล่ออกมา")]
    public float laserEffectDuration = 1.5f;
    [Tooltip("ระยะเวลาที่ Animation ท่าจบ (Player_landing) เล่น")]
    public float playerLandingDuration = 0.5f;

    [Header("Landing Sound")]
    public AudioClip landingSFX;

    public Animator animator;
    private BoxCollider2D boxCollider;

    private AudioSource audioSource;
    private bool isDead = false;
    private int usedLives = 0;
    private bool isInvincible = false;

    // ⭐ NEW: เพิ่มตัวแปรสำหรับ God Mode ที่ Cheat Script จะใช้ ⭐
    public bool isGodModeActive = false;

    private SpriteRenderer playerSpriteRenderer;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // เก็บ Reference Components
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        boxCollider = GetComponent<BoxCollider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();


        // เริ่ม sequence การเกิดตอนเริ่มเกม
        StartCoroutine(PlayLandingSequenceAtSpawn());
    }

    // ⭐ MODIFIED: ตรวจสอบ isGodModeActive ก่อนเสมอ ⭐
    public void TakeDamage(float dmg)
    {
        // ⭐ [FIX]: หากอยู่ใน God Mode ให้ Return ทันที
        if (isGodModeActive)
        {
            Debug.Log("Player is protected by God Mode and took no damage.");
            return;
        }

        if (isDead || isInvincible) return;

        // ไม่ว่าดาเมจจะเป็นเท่าไหร่ (dmg) ก็ตายทันที
        Debug.Log(gameObject.name + " was hit and instantly defeated.");
        Die();
    }

    void Die()
    {
        if (isDead) return; // ป้องกันการเรียกซ้ำ

        // ⭐ NEW: หากกำลังอยู่ใน God Mode ห้ามตาย
        if (isGodModeActive) return;

        isDead = true;

        // ล็อคการควบคุมทันทีที่ตาย
        if (playerMovement != null) playerMovement.isLocked = true;

        // หยุด Coroutine อมตะที่อาจยังทำงานอยู่
        StopCoroutine(RespawnInvincible());
        if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true; // ให้เห็นตัวชัดๆ ตอนตาย

        if (usedLives < autoRespawnLives)
        {
            usedLives++;
            Respawn();
        }
        else
        {
            Debug.Log("Game Over! All lives used.");
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }
    }

    public int UsedLives => usedLives;
    public bool RestoreLives(int amount)
    {
        int remainingLives = autoRespawnLives - usedLives;
        if (remainingLives <= 0) return false;

        usedLives = Mathf.Max(0, usedLives - amount);

        // NEW: หากตายอยู่ ให้ Respawn ทันทีเมื่อได้ Life คืนและยังมี Life เหลือ
        if (isDead && usedLives < autoRespawnLives)
        {
            Respawn();
        }
        return true;
    }

    void Respawn()
    {
        // รีเซ็ตสถานะ
        isDead = false;

        // ย้ายไปจุด respawn
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position + Vector3.up * 3f;
        }

        // เปิด PlayerMovement เพื่อให้ฟิสิกส์ทำงาน (ตัวละครร่วงลงมา)
        if (playerMovement != null) playerMovement.enabled = true;

        // เริ่ม Sequence การเกิด
        StartCoroutine(PlayLandingSequenceAtSpawn());

        // หมายเหตุ: เอา StartCoroutine(RespawnInvincible()) ออกจากตรงนี้ 
        // เพื่อไปเริ่มตอนตัวละครโผล่มาแทน
    }

    IEnumerator PlayLandingSequenceAtSpawn()
    {
        // 1. ล็อคการรับ Input (ห้ามเดิน/กระโดด)
        if (playerMovement != null) playerMovement.isLocked = true;

        // รอให้ตัวละครร่วงลงมาสักพัก (เผื่อต้องร่วงลงไปแตะพื้น)
        yield return new WaitForSeconds(0.25f);

        // 2. ซ่อนตัวละคร (หายตัว)
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = false;

        // 3. สร้างแสงเลเซอร์
        if (laserLandingPrefab != null)
        {
            Vector3 effectPosition = transform.position;

            // หาตำแหน่งเท้า (Bottom of Collider)
            if (boxCollider != null)
                effectPosition = transform.position + new Vector3(0, -boxCollider.size.y / 2f, 0);

            // บวกค่า Offset
            effectPosition += laserSpawnOffset;

            GameObject laserEffect = Instantiate(laserLandingPrefab, effectPosition, Quaternion.identity);
            Destroy(laserEffect, laserEffectDuration);
        }

        // 4. รอให้เลเซอร์แสดงผลจนจบ
        yield return new WaitForSeconds(laserEffectDuration);

        // 5. เลเซอร์จบ -> แสดงตัวละคร (โผล่มา)
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = true;

        // ⭐ เริ่มกระพริบอมตะ "หลังจาก" ตัวละครโผล่มาแล้ว ⭐
        // ต้องตรวจสอบ God Mode ก่อนเริ่มกระพริบปกติ
        if (!isGodModeActive)
        {
            StartCoroutine(RespawnInvincible());
        }

        // เล่น Animation ท่าจบ (Landing)
        if (animator != null)
            animator.SetTrigger("Landing");

        // เล่นเสียง
        if (audioSource != null && landingSFX != null)
            audioSource.PlayOneShot(landingSFX);

        // 6. รอท่าจบเล่นเสร็จ
        yield return new WaitForSeconds(playerLandingDuration);

        // 7. จบ Sequence -> กลับไปท่า Idle และปลดล็อคการควบคุม
        if (animator != null)
            animator.SetTrigger("EndLanding");

        if (playerMovement != null)
            playerMovement.isLocked = false;
    }

    IEnumerator RespawnInvincible()
    {
        isInvincible = true;
        float timer = respawnInvincibleTime;

        while (timer > 0)
        {
            // หาก God Mode ถูกเปิดระหว่างกระพริบ ให้หยุด Coroutine นี้ทันที
            if (isGodModeActive)
            {
                if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true;
                isInvincible = false; // รีเซ็ตสถานะอมตะชั่วคราว
                yield break;
            }

            // สลับเปิด/ปิด Sprite (กระพริบ)
            if (playerSpriteRenderer != null)
                playerSpriteRenderer.enabled = !playerSpriteRenderer.enabled;

            // ความเร็วกระพริบ (ยิ่งน้อยยิ่งรัว)
            yield return new WaitForSeconds(0.1f);
            timer -= 0.1f;
        }

        // จบอมตะ: บังคับเปิด Sprite ให้เห็นตัวแน่นอน
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = true;

        isInvincible = false;
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}