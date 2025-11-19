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

        // 🎯 เปลี่ยน Logic การหาเป้าหมาย: เรียกใช้ฟังก์ชันหาศัตรูที่ใกล้ที่สุด
        target = FindClosestEnemy();

        // ทำลายตัวเองหลังจากเวลาที่กำหนด
        Destroy(gameObject, lifetime);
    }
    Transform FindClosestEnemy()
    {
        // 1. หารายชื่อศัตรูทั้งหมดในฉาก
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform closestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity; // ใช้ Squared Distance เพื่อประหยัดการคำนวณ

        Vector3 currentPosition = transform.position;

        // 2. วนลูปเพื่อหาศัตรูที่ใกล้ที่สุด
        foreach (GameObject enemy in enemies)
        {
            // ตรวจสอบระยะทาง
            Vector3 directionToTarget = enemy.transform.position - currentPosition;
            float dSqr = directionToTarget.sqrMagnitude; // ระยะทางยกกำลังสอง

            if (dSqr < closestDistanceSqr)
            {
                // ถ้าศัตรูตัวนี้ใกล้กว่าตัวที่เคยเจอ ให้เก็บไว้
                closestDistanceSqr = dSqr;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }
    public void Initialize(Vector2 direction)
    {
        initialDirection = direction;
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