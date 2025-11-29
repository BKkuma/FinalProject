using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    

    [Header("UI")]
    public GameObject gameOverUI;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnInvincibleTime = 3f; 
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

    
    public bool isGodModeActive = false;

    private SpriteRenderer playerSpriteRenderer;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);

        
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        boxCollider = GetComponent<BoxCollider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();


       
        StartCoroutine(PlayLandingSequenceAtSpawn());
    }

    
    public void TakeDamage(float dmg)
    {
        
        if (isGodModeActive)
        {
            Debug.Log("Player is protected by God Mode and took no damage.");
            return;
        }

        if (isDead || isInvincible) return;

        
        Debug.Log(gameObject.name + " was hit and instantly defeated.");
        Die();
    }

    void Die()
    {
        if (isDead) return; 

        
        if (isGodModeActive) return;

        isDead = true;

        
        if (playerMovement != null) playerMovement.isLocked = true;

        
        StopCoroutine(RespawnInvincible());
        if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true; 

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

       
        if (isDead && usedLives < autoRespawnLives)
        {
            Respawn();
        }
        return true;
    }

    void Respawn()
    {
        
        isDead = false;

        
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position + Vector3.up * 3f;
        }

        
        if (playerMovement != null) playerMovement.enabled = true;

        
        StartCoroutine(PlayLandingSequenceAtSpawn());

        
    }

    IEnumerator PlayLandingSequenceAtSpawn()
    {
        
        if (playerMovement != null) playerMovement.isLocked = true;

        
        yield return new WaitForSeconds(0.25f);

        
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = false;

        
        if (laserLandingPrefab != null)
        {
            Vector3 effectPosition = transform.position;

            
            if (boxCollider != null)
                effectPosition = transform.position + new Vector3(0, -boxCollider.size.y / 2f, 0);

            
            effectPosition += laserSpawnOffset;

            GameObject laserEffect = Instantiate(laserLandingPrefab, effectPosition, Quaternion.identity);
            Destroy(laserEffect, laserEffectDuration);
        }

       
        yield return new WaitForSeconds(laserEffectDuration);

        
        if (playerSpriteRenderer != null)
            playerSpriteRenderer.enabled = true;

        
        if (!isGodModeActive)
        {
            StartCoroutine(RespawnInvincible());
        }

        
        if (animator != null)
            animator.SetTrigger("Landing");

        
        if (audioSource != null && landingSFX != null)
            audioSource.PlayOneShot(landingSFX);

        
        yield return new WaitForSeconds(playerLandingDuration);

        
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
            
            if (isGodModeActive)
            {
                if (playerSpriteRenderer != null) playerSpriteRenderer.enabled = true;
                isInvincible = false; 
                yield break;
            }

            
            if (playerSpriteRenderer != null)
                playerSpriteRenderer.enabled = !playerSpriteRenderer.enabled;

            
            yield return new WaitForSeconds(0.1f);
            timer -= 0.1f;
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