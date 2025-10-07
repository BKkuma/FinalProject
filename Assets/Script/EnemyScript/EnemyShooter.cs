using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;    // กระสุนของ Enemy
    public Transform firePoint;        // จุดยิงกระสุน
    public float fireRate = 2f;        // ความถี่ในการยิง
    public float bulletSpeed = 7f;     // ความเร็วกระสุน
    public Transform player;           // Reference ไปที่ Player
    public float shootRange = 8f;      // ระยะยิงสูงสุด

    private float nextFireTime;

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootRange)
        {
            // หันหน้าไปทาง Player (เฉพาะแกน X)
            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);

            // ยิงถ้า cooldown ครบ
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void Shoot()
    {
        // ทิศทางจาก Enemy → Player
        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;
    }
}
