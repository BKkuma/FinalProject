using UnityEngine;

public class BulletTurret : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (rb != null && rb.velocity != Vector2.zero)
        {
            // หมุน sprite ของกระสุนให้หันไปตามทิศทางที่เคลื่อนที่
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;

            // 🧭 ปรับ offset ให้ตรงกับ art ของคุณ
            transform.rotation = Quaternion.Euler(0, 0, angle - 0f);
            // ถ้ายังหันไม่ตรง ลองเปลี่ยน -90f เป็น 0f หรือ +90f หรือ +180f
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
