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
        if (playerOnTrigger && !isLifting)
        {
            isLifting = true;

            if (barrier != null) barrier.SetActive(true);

            if (playerCameraFollow != null)
                playerCameraFollow.lockMovementTemporarily = true;

            if (mainCamera != null && arenaCameraPoint != null)
                StartCoroutine(MoveCamera(mainCamera.transform, arenaCameraPoint.position, cameraMoveDuration));
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

    IEnumerator MoveCamera(Transform cam, Vector3 targetPos, float duration)
    {
        Vector3 startPos = cam.position;
        float elapsed = 0f;
        targetPos.z = startPos.z;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = cameraEase.Evaluate(elapsed / duration);
            cam.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        cam.position = targetPos;
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
                boss.StartBossBehavior(); // เริ่ม Coroutine ของบอสครั้งเดียว
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
        if (barrier != null) barrier.SetActive(false);
        StartCoroutine(ReturnCameraToPlayer());
    }

    IEnumerator ReturnCameraToPlayer()
    {
        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = playerCameraFollow.target.position + playerCameraFollow.offset;
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

        if (playerCameraFollow != null)
            playerCameraFollow.lockMovementTemporarily = false;
    }
}
