using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;     // Prefab ของศัตรู
    public Transform[] spawnPoints;    // จุดที่ศัตรูสามารถเกิด
    public int maxEnemies = 10;        // จำนวนศัตรูสูงสุด (spawn ได้สูงสุดกี่ครั้ง)
    public float spawnDelay = 2f;      // เวลาหน่วงก่อน Spawn ตัวใหม่
    public float activationRange = 8f; // ระยะที่ Player ต้องเข้ามาใกล้ถึงจะเริ่ม Spawn

    private int enemiesSpawned = 0;    // นับจำนวนที่ spawn ไปแล้ว
    private GameObject currentEnemy;   // เก็บ Enemy ที่ spawn ปัจจุบัน
    private float spawnTimer = 0f;     // ตัวจับเวลา
    private Transform player;          // เก็บ reference ของ Player
    private bool isActive = false;     // Spawner เริ่มทำงานหรือยัง

    void Start()
    {
        // หาตัว Player (tag ต้องตั้งเป็น "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // ถ้าเจอ Player ให้เช็คระยะก่อน
        if (player != null && !isActive)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= activationRange)
            {
                isActive = true; // เริ่มทำงาน
            }
        }

        if (!isActive) return; // ถ้ายังไม่ active → ไม่ต้องทำงาน

        // ถ้ายังไม่ครบจำนวนสูงสุด และไม่มี enemy อยู่ในฉาก
        if (currentEnemy == null && enemiesSpawned < maxEnemies)
        {
            // นับเวลา
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnDelay)
            {
                SpawnEnemy();
                spawnTimer = 0f; // รีเซ็ตเวลา
            }
        }
    }

    void SpawnEnemy()
    {
        // เลือกตำแหน่งสุ่มจาก spawnPoints
        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        // สร้าง Enemy
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        enemiesSpawned++;

        // ผูก event เมื่อ Enemy ตาย ให้ clear currentEnemy
        EnemyHealth health = currentEnemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.onDeath += () => currentEnemy = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // วาดวงกลมแสดงระยะ activation
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
