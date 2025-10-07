using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI; // UI Game Over

    [Header("Respawn Settings")]
    public Transform respawnPoint;    // จุดเกิดใหม่
    public float respawnInvincibleTime = 5f; // เวลาที่อมตะหลังเกิด
    public int autoRespawnLives = 3; // Buddy เกิดอัตโนมัติได้กี่ครั้ง

    private bool isDead = false;
    private int usedLives = 0;
    private bool isInvincible = false;

    private SpriteRenderer sr;

    void Start()
    {
        isDead = false;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        sr = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || isInvincible) return;

        Debug.Log("Player took damage, dead instantly!");
        Die();
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // ปิดการเคลื่อนไหวชั่วคราว
        GetComponent<PlayerMovement>().enabled = false;

        // เช็คว่าใช้ Auto Respawn หมดหรือยัง
        if (usedLives < autoRespawnLives)
        {
            usedLives++;
            Invoke(nameof(Respawn), 0.0f); // delay ให้เหมือนเด้งลงมา
        }
        else
        {
            // หมดชีวิต → แสดง GameOver
            if (gameOverUI != null)
                gameOverUI.SetActive(true);
        }
    }

    void Respawn()
    {
        isDead = false;

        // ย้ายไปจุด respawn (ให้สูงกว่าหน่อย เพื่อให้เหมือนตกลงมา)
        transform.position = respawnPoint.position + new Vector3(0, 3f, 0);

        // เปิดการเคลื่อนไหว
        GetComponent<PlayerMovement>().enabled = true;

        // เปิดโหมดอมตะชั่วคราว
        StartCoroutine(RespawnInvincible());
    }

    System.Collections.IEnumerator RespawnInvincible()
    {
        isInvincible = true;
        float timer = respawnInvincibleTime;

        while (timer > 0)
        {
            // ทำ effect กระพริบ
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.2f);
            timer -= 0.2f;
        }

        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    void Update()
    {
        // **ไม่ต้องกด R เพื่อ Respawn แล้ว**
        // GameOver จะใช้ปุ่มจาก UI แทน
    }

    // ปุ่ม Try Again ใน GameOver UI จะเรียกใช้ฟังก์ชันนี้
    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
