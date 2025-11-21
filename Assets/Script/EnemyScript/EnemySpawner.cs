using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;      // Prefab ของศัตรู
    public Transform[] spawnPoints;     // จุดที่ศัตรูสามารถเกิด
    public int maxEnemies = 10;         // จำนวนศัตรูสูงสุดที่จะ Spawn ได้ทั้งหมด
    public float spawnDelay = 2f;       // เวลาหน่วงก่อน Spawn ตัวใหม่
    public float activationRange = 8f;  // ระยะที่ Player ต้องเข้ามาใกล้

    private int enemiesSpawned = 0;     // นับจำนวนที่ spawn ไปแล้ว
    private GameObject currentEnemy;    // เก็บ Enemy ตัวล่าสุด
    private float spawnTimer = 0f;
    private Transform player;
    private bool isActive = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // เช็คระยะ Player
        if (player != null && !isActive)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= activationRange)
            {
                isActive = true;
            }
        }

        if (!isActive) return;

        // เงื่อนไขการ Spawn:
        // 1. ยังไม่ครบจำนวน Max
        // 2. (Optional) ถ้าอยากให้มีทีละตัว ให้เช็ค currentEnemy == null ด้วย
        //    แต่ถ้าอยากให้ Spawn ออกมาเรื่อยๆ ตามเวลา ให้ลบเงื่อนไข currentEnemy == null ออก
        if (currentEnemy == null && enemiesSpawned < maxEnemies)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnDelay)
            {
                SpawnEnemy();
                spawnTimer = 0f;
            }
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        // เลือกจุดเกิดสุ่ม
        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        // สร้าง Enemy
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        enemiesSpawned++;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}