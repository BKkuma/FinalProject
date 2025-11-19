using UnityEngine;

public class ArenaController : MonoBehaviour
{
    [Header("Arena Settings")]
    public Transform liftablePlatform;
    public float liftHeight = 3f;
    public float liftSpeed = 2f;
    public GameObject barrier;
    public Transform bossSpawnPoint;
    public GameObject bossObject;

    [Header("Camera")]
    public Camera mainCamera;
    public Camera arenaCamera;

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
            barrier.SetActive(false);

        if (bossObject != null)
            bossObject.SetActive(false);

        // กล้องปกติเปิดอยู่ -> กล้อง arena ปิด
        if (arenaCamera != null) arenaCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
    }

    void Update()
    {
        if (playerOnTrigger && !isLifting)
        {
            isLifting = true;

            if (barrier != null)
                barrier.SetActive(true);

            // เปลี่ยนกล้องไป ArenaCamera
            SwapToArenaCamera();
        }

        if (isLifting && liftablePlatform != null)
        {
            liftablePlatform.position = Vector3.MoveTowards(
                liftablePlatform.position, targetPos, liftSpeed * Time.deltaTime);

            if (Vector3.Distance(liftablePlatform.position, targetPos) < 0.01f)
            {
                SpawnBoss();
                isLifting = false;
            }
        }
    }

    void SwapToArenaCamera()
    {
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (arenaCamera != null) arenaCamera.gameObject.SetActive(true);
        Debug.Log("Switched to Arena Camera");
    }

    void SwapToMainCamera()
    {
        if (arenaCamera != null) arenaCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        Debug.Log("Switched back to Main Camera");
    }

    void SpawnBoss()
    {
        if (bossObject != null && bossSpawnPoint != null)
        {
            bossObject.transform.position = bossSpawnPoint.position;
            bossObject.SetActive(true);

            // เชื่อม event ให้ถูกต้อง
            JetBossPhase1 boss = bossObject.GetComponent<JetBossPhase1>();
            if (boss != null)
                boss.onPhase1Finished = OnBossDefeated;

            Debug.Log("Boss Spawned!");
        }
    }

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

    public void OnBossDefeated()
    {
        Debug.Log("Boss defeated!");

        // ปลดกำแพง
        if (barrier != null)
            barrier.SetActive(false);

        // เปลี่ยนกลับไป MainCamera
        SwapToMainCamera();
    }
}
