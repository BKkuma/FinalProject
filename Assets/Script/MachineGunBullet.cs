using UnityEngine;

public class MachineGunBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 5f;

    [HideInInspector]
    public Vector2 direction = Vector2.right; // จะถูกเซ็ตจาก PlayerShooting

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;

        // ปรับ rotation ให้หัวกระสุนชี้ไปทิศทางยิง
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // กำหนดความเร็ว
        if (rb != null) rb.velocity = direction * speed;

        // ทำลายหลังเวลา
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HelicopterBoss boss = collision.gameObject.GetComponent<HelicopterBoss>();
            if (boss != null) boss.TakeDamage(damage);

            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null) enemy.TakeDamage(damage);

            EnemyFlyingAI flyingEnemy = collision.gameObject.GetComponent<EnemyFlyingAI>();
            if (flyingEnemy != null) flyingEnemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
