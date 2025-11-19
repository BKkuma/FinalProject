using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    public float speed = 5f;
    public float rotateSpeed = 200f;
    public int damage = 1;
    public float lifetime = 5f;

    [HideInInspector]
    public Vector2 initialDirection = Vector2.right; // ← ทิศทางเริ่มต้น

    private Rigidbody2D rb;
    private Transform target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // หาเป้าหมายที่มี tag เป็น Enemy
        GameObject enemyObj = GameObject.FindWithTag("Enemy");
        if (enemyObj != null)
            target = enemyObj.transform;

        // ทำลายตัวเองหลังจากเวลาที่กำหนด
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.velocity = initialDirection.normalized * speed; // ยิงตรงตามทิศทาง Player
            return;
        }

        // หมุนกระสุนเข้าหาเป้าหมาย
        Vector2 direction = (Vector2)target.position - rb.position;
        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rb.angularVelocity = -rotateAmount * rotateSpeed;
        rb.velocity = transform.up * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // บอสเก่า
            HelicopterBoss boss = collision.gameObject.GetComponent<HelicopterBoss>();
            if (boss != null) boss.TakeDamage(damage);

            // ** บอส Phase 2 ที่คุณต้องการเพิ่ม **
            Boss2 boss2 = collision.gameObject.GetComponent<Boss2>();
            if (boss2 != null) boss2.TakeDamage(damage);

            // ศัตรูภาคพื้น
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null) enemy.TakeDamage(damage);

            // ศัตรูบิน
            EnemyFlyingAI flyingEnemy = collision.gameObject.GetComponent<EnemyFlyingAI>();
            if (flyingEnemy != null) flyingEnemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}