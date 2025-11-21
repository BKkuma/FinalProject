using UnityEngine;
using System.Collections;

public class BossActivator : MonoBehaviour
{
    [Header("References")]
    public GameObject boss;                   // บอส
    public Camera mainCamera;                 // กล้องผู้เล่น
    public GameObject player;                 // ผู้เล่น
    public CameraFollowLockY playerCameraFollow; // Follow script ของผู้เล่น
    public Transform bossCameraPoint;         // Empty GameObject ตำแหน่งกล้องบอส
    public GameObject bossBounds;             // กำแพงบอส

    [Header("Camera Settings")]
    public float camMoveDuration = 1.2f;
    public AnimationCurve camEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // เปิดบอส (แต่บอสจะยังไม่ทำอะไรจนกว่าจะเรียก StartBossSequence)
            if (boss != null)
                boss.SetActive(true);

            // เปิด bossBounds ทันที
            if (bossBounds != null)
                bossBounds.SetActive(true);

            // ปิด PlayerBounds ชั่วคราว
            PlayerBounds pb = player.GetComponent<PlayerBounds>();
            if (pb != null)
                pb.enabled = false;

            // เริ่มเลื่อนกล้อง
            StartCoroutine(MoveCameraToBoss(pb));
        }
    }

    IEnumerator MoveCameraToBoss(PlayerBounds pb)
    {
        // ปิด follow ผู้เล่นชั่วคราว
        if (playerCameraFollow != null)
            playerCameraFollow.enabled = false;

        // เก็บตำแหน่งเริ่มต้น mainCamera
        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = bossCameraPoint.position;
        endPos.z = startPos.z; // รักษา Z ของ mainCamera

        // 1. เลื่อนกล้องไปหาบอส
        float timer = 0f;
        while (timer < camMoveDuration)
        {
            timer += Time.deltaTime;
            float t = camEase.Evaluate(timer / camMoveDuration);
            Vector3 tempPos = Vector3.Lerp(startPos, endPos, t);

            // Clamping Y ขณะกล้องเลื่อน
            float clampedY = Mathf.Clamp(tempPos.y, playerCameraFollow.minY, playerCameraFollow.maxY);
            mainCamera.transform.position = new Vector3(tempPos.x, clampedY, tempPos.z);

            yield return null;
        }

        // 2. เปลี่ยนเพลงบอส
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayBossMusic();

        // 3. สั่งให้บอสเล่นฉากเปิดตัว (บินเข้ามา)
        HelicopterBoss heli = boss.GetComponent<HelicopterBoss>();
        if (heli != null)
        {
            heli.StartBossSequence(); // <--- สั่งบอสบินเข้าฉากตรงนี้

            bool bossDefeated = false;
            heli.onBossDefeated += () => { bossDefeated = true; };
            yield return new WaitUntil(() => bossDefeated);
        }

        // 4. กลับตำแหน่งผู้เล่น (หลังจากบอสตาย)
        startPos = mainCamera.transform.position;
        Vector3 endPosToPlayer = player.transform.position + (playerCameraFollow != null ? playerCameraFollow.offset : Vector3.zero);
        endPosToPlayer.z = startPos.z;

        timer = 0f;
        while (timer < camMoveDuration)
        {
            timer += Time.deltaTime;
            float t = camEase.Evaluate(timer / camMoveDuration);
            Vector3 tempPos = Vector3.Lerp(startPos, endPosToPlayer, t);

            float clampedY = Mathf.Clamp(tempPos.y, playerCameraFollow.minY, playerCameraFollow.maxY);
            mainCamera.transform.position = new Vector3(tempPos.x, clampedY, tempPos.z);

            yield return null;
        }

        // 5. เปิด follow ผู้เล่นอีกครั้ง
        if (playerCameraFollow != null)
        {
            Vector3 newTargetPos = player.transform.position;
            playerCameraFollow.TeleportToTarget(newTargetPos);
            playerCameraFollow.ResetLockToTarget();
            playerCameraFollow.enabled = true;
        }

        if (pb != null) pb.enabled = true;
        if (bossBounds != null) bossBounds.SetActive(false);

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayNormalMusic();
    }
}