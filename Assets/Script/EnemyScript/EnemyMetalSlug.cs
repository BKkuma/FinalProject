using UnityEngine;

public class EnemyMetalSlug : MonoBehaviour
{
    public float detectionRange = 8f;   // ระยะตรวจจับ Player
    public float fireRate = 1.5f;       // ยิงทุกๆ กี่วิ
    public GameObject bulletPrefab;     // กระสุนศัตรู
    public Transform firePoint;         // จุดยิงกระสุน
    private Transform player;
    private float nextFireTime;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            // หันไปทาง Player
            if (player.position.x < transform.position.x)
                spriteRenderer.flipX = true;   // หันซ้าย
            else
                spriteRenderer.flipX = false;  // หันขวา

            // ยิงเมื่อถึงเวลา
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector2 shootDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = shootDir * 7f;
    }
}
