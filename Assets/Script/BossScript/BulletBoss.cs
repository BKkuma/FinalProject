using UnityEngine;

public class BulletBoss : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f;

    [HideInInspector]
    public Vector2 direction = Vector2.zero; // ⭐ MODIFIED: เปลี่ยนค่าเริ่มต้นเป็น Vector2.zero

    private Rigidbody2D rb;

    void Awake()
    {
        // รับ Rigidbody2D ที่ Awake เพื่อให้พร้อมใช้ใน SetDirection
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // ลบโค้ดเคลื่อนที่ออกจาก Start()
        Destroy(gameObject, lifeTime);
    }

    // ⭐ NEW: ฟังก์ชันนี้ให้บอสเรียกเพื่อกำหนดทิศทางและความเร็ว
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        if (rb != null)
        {
            rb.velocity = direction * speed; // ⭐ เริ่มเคลื่อนที่ตามทิศทางที่บอสสั่ง
        }
        else
        {
            Debug.LogWarning("BulletBoss requires a Rigidbody2D component to move!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        // ตรวจสอบกับสิ่งอื่นๆ ที่ต้องการทำลายกระสุน
        // if (other.CompareTag("Environment"))
        // {
        //     Destroy(gameObject);
        // }
    }
}