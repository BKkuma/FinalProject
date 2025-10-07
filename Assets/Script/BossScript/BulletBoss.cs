using UnityEngine;

public class BulletBoss : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f;

    [HideInInspector]
    public Vector2 direction = Vector2.down; // ����������纷�ȷҧ

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = direction * speed; // �� direction ᷹ Vector2.down

        Destroy(gameObject, lifeTime);
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
