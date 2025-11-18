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

    [Header("Landing Effect")]
    public Animator animator;
    public GameObject landingEffectPrefab;

    [Header("Landing Sound")]
    public AudioClip landingSFX;
    private AudioSource audioSource;

    private bool isDead = false;
    private int usedLives = 0;
    private bool isInvincible = false;
    private SpriteRenderer sr;

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);

        sr = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // เล่น Landing ตอนเริ่มเกม
        StartCoroutine(PlayLandingEffectAtSpawn());
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || isInvincible) return;
        Die();
    }

    void Die()
    {
        isDead = true;
        GetComponent<PlayerMovement>().enabled = false;

        if (usedLives < autoRespawnLives)
        {
            usedLives++;
            Invoke(nameof(Respawn), 0.0f);
        }
        else
        {
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }
    }

    public int UsedLives => usedLives;

    void Respawn()
    {
        isDead = false;

        // ย้ายไปจุด respawn
        transform.position = respawnPoint.position + Vector3.up * 3f;

        // ปิด movement ชั่วคราว
        GetComponent<PlayerMovement>().enabled = false;

        // เปิด movement ให้ตกลงพื้น
        GetComponent<PlayerMovement>().enabled = true;

        // เล่น Landing animation / effect / sound
        StartCoroutine(PlayLandingEffectAtSpawn());

        // โหมดอมตะชั่วคราว
        StartCoroutine(RespawnInvincible());
    }

    IEnumerator PlayLandingEffectAtSpawn()
    {
        // รอให้ player ตกลงพื้นประมาณ 0.2-0.3 วิ (ปรับตามความสูง respawn)
        yield return new WaitForSeconds(0.25f);

        // เล่น Animation
        if (animator != null)
            animator.SetTrigger("Landing");

        // Spawn particle effect
        if (landingEffectPrefab != null)
        {
            GameObject effect = Instantiate(landingEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // เล่นเสียง
        if (audioSource != null && landingSFX != null)
            audioSource.PlayOneShot(landingSFX);
    }


    IEnumerator RespawnInvincible()
    {
        isInvincible = true;
        float timer = respawnInvincibleTime;

        while (timer > 0)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.2f);
            timer -= 0.2f;
        }

        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
