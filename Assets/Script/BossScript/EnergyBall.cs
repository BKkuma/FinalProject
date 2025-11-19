using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    public float fallSpeed = 8f;
    public int damage = 10;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // ให้เริ่มตกเร็วขึ้นเล็กน้อย
        rb.velocity = Vector2.down * fallSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🎯 ตรวจสอบการชนกับผู้เล่น
        if (other.CompareTag("Player"))
        {
            // 1. ดึงสคริปต์ PlayerHealth
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // 2. ถ้าเจอสคริปต์ ให้เรียกฟังก์ชันทำดาเมจ
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); // ทำดาเมจตามค่า damage ที่ตั้งไว้
            }

            Debug.Log("Player hit by energy ball!");
            Destroy(gameObject); // ทำลายตัวเอง
        }
        // 🎯 ตรวจสอบการชนกับพื้น
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject); // ทำลายตัวเอง
        }
    }
}