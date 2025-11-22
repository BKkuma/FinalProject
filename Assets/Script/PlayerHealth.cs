using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnInvincibleTime = 5f;
    public int autoRespawnLives = 3;

    [Header("Laser Landing Effect")]
    [Tooltip("Prefab ของลำแสงที่ Instatiate ออกมา (ลำแสงต้องหายไปเองเมื่อเวลาผ่านไป)")]
    public GameObject laserLandingPrefab;
    [Tooltip("ระยะเวลาตั้งแต่ลำแสงเริ่มจนกระทั่งตัวละครโผล่ออกมา (ควรเท่ากับเวลาที่ลำแสงหายไป)")]
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

    private SpriteRenderer playerSpriteRenderer;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);

        // เก็บ Reference Components ที่จำเป็น
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        boxCollider = GetComponent<BoxCollider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // เล่น Landing Sequence ตอนเริ่มเกม
        StartCoroutine(PlayLandingSequenceAtSpawn());
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || isInvincible) return;
        Die();
    }

    void Die()
    {
        isDead = true;

        // ⭐ แก้ไข: ล็อคการควบคุมด้วย isLocked
        if (playerMovement != null) playerMovement.isLocked = true;

        if (usedLives < autoRespawnLives)
        {
            usedLives++;
            Respawn();
        }
        else
        {
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }
    }

    public int UsedLives => usedLives;
    public bool RestoreLives(int amount)
    {
        int remainingLives = autoRespawnLives - usedLives;
        if (remainingLives >= autoRespawnLives)
        {
            return false;
        }
        usedLives = Mathf.Max(0, usedLives - amount);
        return true;
    }

    void Respawn()
    {
        isDead = false;

        // ย้ายไปจุด respawn
        transform.position = respawnPoint.position + Vector3.up * 3f;

        // ⭐ แก้ไข: เปิด PlayerMovement ทันทีเพื่อให้ฟิสิกส์ทำงาน (ตัวละครเริ่มร่วง)
        if (playerMovement != null) playerMovement.enabled = true;

        // เล่น Landing Sequence
        StartCoroutine(PlayLandingSequenceAtSpawn());

        // โหมดอมตะชั่วคราว
        StartCoroutine(RespawnInvincible());
    }

    IEnumerator PlayLandingSequenceAtSpawn()
    {
        // A. ⭐ ลำดับที่ 1: ซ่อนตัวละครและแสดงลำแสง ⭐

        // ⭐ NEW: ล็อคการรับ Input ทันทีเพื่อไม่ให้ผู้เล่นขยับขณะร่วง ⭐
        if (playerMovement != null) playerMovement.isLocked = true;

        // A1. รอให้ player ตกลงจาก RespawnPoint ถึงตำแหน่งที่ลำแสงควรเริ่ม
        yield return new WaitForSeconds(0.25f);

        // A2. ซ่อนโมเดลตัวละครหลัก
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = false;

        // A3. Instantiate ลำแสง
        if (laserLandingPrefab != null)
        {
            Vector3 effectPosition = transform.position;
            if (boxCollider != null)
            {
                effectPosition = transform.position + new Vector3(0, -boxCollider.size.y / 2f, 0);
            }

            GameObject laserEffect = Instantiate(laserLandingPrefab, effectPosition, Quaternion.identity);

            Destroy(laserEffect, laserEffectDuration);
        }

        // B. ⭐ ลำดับที่ 2: รอให้ลำแสงหายไปจนหมด ⭐
        yield return new WaitForSeconds(laserEffectDuration);

        // C. ⭐ ลำดับที่ 3: แสดงตัวละครและเล่นท่าจบ ⭐

        // C1. แสดงโมเดลตัวละครหลัก
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = true;

        // C2. เล่น Animation ท่าจบ (Player_landing)
        if (animator != null)
            // ใช้ SetTrigger "Landing"
            animator.SetTrigger("Landing");

        // C3. เล่นเสียง
        if (audioSource != null && landingSFX != null)
            audioSource.PlayOneShot(landingSFX);

        // D. ⭐ ลำดับที่ 4: รอให้ Animation ท่าจบเล่นจนเสร็จ ⭐
        yield return new WaitForSeconds(playerLandingDuration);

        // E. ปลดล็อคการควบคุม

        // ⭐ NEW: สั่งให้ Animator กลับไป Idle ทันที ⭐
        if (animator != null)
            animator.SetTrigger("EndLanding");

        if (playerMovement != null)
        {
            // ปลดล็อคการรับ Input 
            playerMovement.isLocked = false;
        }
    }


    IEnumerator RespawnInvincible()
    {
        isInvincible = true;
        float timer = respawnInvincibleTime;

        while (timer > 0)
        {
            // ตรวจสอบว่า SpriteRenderer ถูกเปิดใช้งาน
            if (playerSpriteRenderer != null && playerSpriteRenderer.enabled)
                playerSpriteRenderer.enabled = !playerSpriteRenderer.enabled;

            yield return new WaitForSeconds(0.2f);
            timer -= 0.2f;
        }

        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = true;

        isInvincible = false;
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}