using UnityEngine;

public class ArenaController : MonoBehaviour
{
    [Header("Arena Settings")]
    public Transform liftablePlatform;      // พื้นที่จะลอยขึ้น
    public float liftHeight = 3f;           // ความสูงที่จะลอย
    public float liftSpeed = 2f;            // ความเร็วยก
    public GameObject barrier;              // สิ่งกั้นไม่ให้ player ออก
    public Transform bossSpawnPoint;        // จุดให้บอสปรากฎ
    public GameObject bossObject;           // บอสใน Scene (Hierarchy) ที่จะเปิด

    private bool playerOnTrigger = false;
    private bool isLifting = false;
    private Vector3 originalPos;
    private Vector3 targetPos;

    void Start()
    {
        if (liftablePlatform != null)
            originalPos = liftablePlatform.position;

        targetPos = originalPos + Vector3.up * liftHeight;

        if (barrier != null)
            barrier.SetActive(false); // ปิดไว้ก่อน

        if (bossObject != null)
            bossObject.SetActive(false); // ปิดบอสก่อน
    }

    void Update()
    {
        if (playerOnTrigger && !isLifting)
        {
            isLifting = true;
            if (barrier != null) barrier.SetActive(true);
            Debug.Log("เริ่มยกพื้น!");
        }

        if (isLifting && liftablePlatform != null)
        {
            liftablePlatform.position = Vector3.MoveTowards(
                liftablePlatform.position, targetPos, liftSpeed * Time.deltaTime);

            if (Vector3.Distance(liftablePlatform.position, targetPos) < 0.01f)
            {
                Debug.Log("พื้นยกสูงสุดแล้ว! Spawn Boss");
                SpawnBoss();
                isLifting = false;
            }
        }
    }

    void SpawnBoss()
    {
        if (bossObject != null && bossSpawnPoint != null)
        {
            bossObject.transform.position = bossSpawnPoint.position;
            bossObject.SetActive(true); // เปิดบอส
            Debug.Log("Boss Spawned!");
        }
        else
        {
            Debug.LogError("BossObject หรือ BossSpawnPoint ยังไม่ได้ assign!");
        }
    }

    // Trigger ตรวจผู้เล่นยืน
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerOnTrigger = true;
            Debug.Log("Player on trigger!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerOnTrigger = false;
    }

    // เมื่อบอสตาย
    public void OnBossDefeated()
    {
        Debug.Log("Boss defeated! สามารถปลด Barrier หรือ spawn Phase 2 ได้");
        if (barrier != null)
            barrier.SetActive(false);
    }
}
