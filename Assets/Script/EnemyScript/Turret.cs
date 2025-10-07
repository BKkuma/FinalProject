using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    [Header("References")]
    public Transform gun;              // ตัวปืนที่จะหมุน
    public Transform firePoint;        // จุดยิงกระสุน
    public GameObject bulletPrefab;    // กระสุนที่จะยิง
    private Transform player;

    [Header("Combat")]
    public float attackRange = 8f;     // ระยะที่เริ่มโจมตี
    public float fireDelay = 3f;       // ยิงทุกกี่วิ
    public float bulletSpeed = 5f;

    [Header("Stats")]
    public int maxHP = 10;
    private int currentHP;

    private bool playerInRange = false;
    private bool isAttacking = false;

    void Start()
    {
        currentHP = maxHP;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= attackRange;

        // หมุนปืนตามผู้เล่น
        if (playerInRange && gun != null)
        {
            Vector2 direction = (player.position - gun.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gun.rotation = Quaternion.Euler(0, 0, angle - -90f);
        }

        // เริ่มยิงถ้ายังไม่กำลังยิง
        if (playerInRange && !isAttacking)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(fireDelay); // รอก่อนยิงครั้งแรก

        while (playerInRange)
        {
            Shoot();
            yield return new WaitForSeconds(fireDelay);
        }

        isAttacking = false;
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null && player != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Vector2 direction = (player.position - firePoint.position).normalized;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = direction * bulletSpeed;
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            Destroy(gameObject); // ระเบิดป้อม
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Bullet playerBullet = collision.gameObject.GetComponent<Bullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.damage);
            }
            Destroy(collision.gameObject);
        }
    }

    // วาด Gizmo แสดงระยะการมองเห็นของป้อม
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
