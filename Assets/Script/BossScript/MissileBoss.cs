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
        if (rb == null) rb = GetComponent<Rigidbody2D>(); // ตรวจอีกชั้น
        Vector3 dir = (playerPos - transform.position).normalized + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
        rb.velocity = dir * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Explode();
        }
        else if (other.CompareTag("Ground"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // ใส่ Particle / Effect ระเบิดตรงนี้ได้
        Destroy(gameObject);
    }
}
