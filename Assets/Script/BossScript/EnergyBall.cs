using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    public float fallSpeed = 8f;
    public int damage = 10;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.rotation = Quaternion.Euler(0, 0, 0); 
        rb.velocity = Vector2.down * fallSpeed;
    }

    // ********** ใช้ OnCollisionEnter2D แทน OnTriggerEnter2D **********
    // ใช้สำหรับการชนที่ Collider ไม่ได้ตั้งค่า Is Trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ใช้ collision.gameObject.CompareTag เพื่อตรวจ Tag ของ Object ที่ชน
        Debug.Log("EnergyBall collided with object tagged: " + collision.gameObject.tag);

        // 🎯 ตรวจสอบการชนกับผู้เล่น
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. ดึงสคริปต์ PlayerHealth
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            // 2. ถ้าเจอสคริปต์ ให้เรียกฟังก์ชันทำดาเมจ
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); 
                Debug.Log("Player HP Reduced! Damage: " + damage);
            }
            else
            {
                Debug.LogError("PlayerHealth script not found on Player GameObject!");
            }

            Destroy(gameObject); // ทำลายตัวเอง
        }
        // 🎯 ตรวจสอบการชนกับพื้น
        // ในเกมแนว Platformer "พื้น" มักจะไม่มี Rigidbody2D และจะชนแบบ "แข็ง"
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject); // ทำลายตัวเอง
        }
    }
    // *************************************************************
    
    // (ลบ OnTriggerEnter2D เดิมออกไป)
}