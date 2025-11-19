using UnityEngine;
using System.Collections;

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
    public CameraFollowLockY playerCameraFollow;
    public Transform arenaCameraPoint;
    public float cameraMoveDuration = 1f;
    public AnimationCurve cameraEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool playerOnTrigger = false;
    private bool isLifting = false;
    private Vector3 originalPlatformPos;
    private Vector3 targetPlatformPos;
    private bool bossBehaviorStarted = false;

    // [แก้ไข 1] เพิ่มตัวแปรนี้เพื่อเช็คว่า Event ทำงานไปแล้วหรือยัง กันการรันซ้ำ
    private bool hasActivated = false;

    void Start()
    {
        if (liftablePlatform != null)
        {
            originalPlatformPos = liftablePlatform.position;
            targetPlatformPos = originalPlatformPos + Vector3.up * liftHeight;
        }

        if (barrier != null) barrier.SetActive(false);
        if (bossObject != null) bossObject.SetActive(false);
    }

    void Update()
    {
        // [แก้ไข 2] เพิ่มเงื่อนไข !hasActivated
        if (playerOnTrigger && !isLifting && !hasActivated)
        {
            hasActivated = true; // สั่งให้เป็นจริงทันที เพื่อไม่ให้เข้าลูปนี้อีก
            isLifting = true;

            if (barrier != null) barrier.SetActive(true);

            StartCoroutine(MoveCameraToArena());
        }

        if (isLifting && liftablePlatform != null)
        {
            liftablePlatform.position = Vector3.MoveTowards(
                liftablePlatform.position, targetPlatformPos, liftSpeed * Time.deltaTime);

            if (Vector3.Distance(liftablePlatform.position, targetPlatformPos) < 0.01f)
            {
                SpawnBoss();
                isLifting = false;
            }
        }
    }

    IEnumerator MoveCameraToArena()
    {
        if (playerCameraFollow != null)
            playerCameraFollow.enabled = false;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = arenaCameraPoint.position;
        endPos.z = startPos.z;

        float elapsed = 0f;
        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = cameraEase.Evaluate(elapsed / cameraMoveDuration);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        mainCamera.transform.position = endPos;
    }

    void SpawnBoss()
    {
        if (bossObject != null && bossSpawnPoint != null && !bossBehaviorStarted)
        {
            bossObject.transform.position = bossSpawnPoint.position;
            bossObject.SetActive(true);

            JetBossPhase1 boss = bossObject.GetComponent<JetBossPhase1>();
            if (boss != null)
            {
                boss.onPhase1Finished = OnBossDefeated;
                boss.StartBossBehavior();
                bossBehaviorStarted = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerOnTrigger = true;
    }

    public void OnBossDefeated()
    {
        if (barrier != null) Destroy(barrier);

        // ปิด CameraFollowLockY ก่อนเลื่อนกล้องกลับ (เผื่อไว้)
        if (playerCameraFollow != null)
            playerCameraFollow.enabled = false;

        StartCoroutine(ReturnCameraToPlayer());
    }

    IEnumerator ReturnCameraToPlayer()
    {
        Debug.Log("Start Returning Camera...");
        Vector3 startPos = mainCamera.transform.position;

        // ใช้ Target เดิมที่ผูกไว้ใน Inspector เลย ไม่ต้อง Find ใหม่
        Vector3 endPos = Vector3.zero;
        if (playerCameraFollow != null && playerCameraFollow.target != null)
        {
            endPos = playerCameraFollow.target.position + playerCameraFollow.offset;
        }
        else
        {
            // กันเหนียว: ถ้าไม่มี Target จริงๆ ค่อยหา
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) endPos = p.transform.position;
        }

        endPos.z = startPos.z;

        float elapsed = 0f;
        while (elapsed < cameraMoveDuration)
        {
            // [แก้ไข 3] ใช้ unscaledDeltaTime เผื่อกรณีเกมหยุดเวลาตอนชนะ
            elapsed += Time.unscaledDeltaTime;
            float t = cameraEase.Evaluate(elapsed / cameraMoveDuration);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        mainCamera.transform.position = endPos;

        Debug.Log("Camera Returned. Re-enabling script...");

        // [แก้ไข 4] บังคับเปิดกลับทันที
        if (playerCameraFollow != null)
        {
            // เช็ค Target อีกรอบ
            if (playerCameraFollow.target == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) playerCameraFollow.target = p.transform;
            }

            playerCameraFollow.ResetLockToTarget(); // รีเซ็ตค่า Lock X (สำคัญมาก)
            playerCameraFollow.enabled = true;      // เปิดสคริปต์

            Debug.Log("Camera Script ENABLED Success.");
        }
    }
}