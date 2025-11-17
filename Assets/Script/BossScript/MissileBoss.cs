using UnityEngine;

public class MissileBoss : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 2;
    public float lifeTime = 6f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // เรียกตอน Instantiate จาก Boss
    public void SetDirection(Vector3 playerPos)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // คำนวณทิศทางพื้นฐาน
        Vector3 dir = (playerPos - transform.position).normalized;

        // เพิ่มความสุ่ม
        dir += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0);
        dir.Normalize();

        // ตั้งค่าความเร็ว
        rb.velocity = dir * speed;

        // ✅ หมุนหัวจรวดตาม "ทิศทางสุดท้ายจริงๆ"
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Explode();
        }
        
    }

    void Explode()
    {
        Destroy(gameObject);
    }
}
