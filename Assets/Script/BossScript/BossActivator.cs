using UnityEngine;
using System.Collections;

public class BossActivator : MonoBehaviour
{
    [Header("References")]
    public GameObject boss;                   // บอส
    public Camera mainCamera;                 // กล้องผู้เล่น
    public GameObject player;                 // ผู้เล่น
    public CameraFollowLockY playerCameraFollow; // Follow script ของผู้เล่น
    public Transform bossCameraPoint;         // Empty GameObject ตำแหน่งกล้องบอส
    public GameObject bossBounds;             // กำแพงบอส

    [Header("Camera Settings")]
    public float camMoveDuration = 1.2f;
    public AnimationCurve camEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // เปิดบอส
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

        // เลื่อนกล้องไป boss
        float timer = 0f;
        while (timer < camMoveDuration)
        {
            timer += Time.deltaTime;
            float t = camEase.Evaluate(timer / camMoveDuration);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        mainCamera.transform.position = endPos;

        // เปลี่ยนเพลงบอส
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayBossMusic();

        // รอ boss ตาย
        HelicopterBoss heli = boss.GetComponent<HelicopterBoss>();
        if (heli != null)
        {
            bool bossDefeated = false;
            heli.onBossDefeated += () => { bossDefeated = true; };
            yield return new WaitUntil(() => bossDefeated);
        }

        // กลับตำแหน่งผู้เล่น
        startPos = mainCamera.transform.position;
        // คำนวณ endPos เพื่อใช้ในการ Lerp เท่านั้น
        Vector3 endPosToPlayer = player.transform.position + (playerCameraFollow != null ? playerCameraFollow.offset : Vector3.zero);
        endPosToPlayer.z = startPos.z;

        timer = 0f;
        while (timer < camMoveDuration)
        {
            timer += Time.deltaTime;
            float t = camEase.Evaluate(timer / camMoveDuration);

            Vector3 tempPos = Vector3.Lerp(startPos, endPosToPlayer, t);

            // [FIX 1] Clamping Y ขณะกล้องเลื่อน (ใช้ minY/maxY จาก CameraFollowLockY)
            float clampedY = Mathf.Clamp(tempPos.y, playerCameraFollow.minY, playerCameraFollow.maxY);
            mainCamera.transform.position = new Vector3(tempPos.x, clampedY, tempPos.z);

            yield return null;
        }

        // เปิด follow ผู้เล่นอีกครั้ง
        if (playerCameraFollow != null)
        {
            // [FIX] ใช้ TeleportToTarget แทนการตั้งค่า position โดยตรง
            Vector3 newTargetPos = player.transform.position; // ตำแหน่งผู้เล่น
            playerCameraFollow.TeleportToTarget(newTargetPos); // กล้องจะ Clamping ตำแหน่งทันที

            playerCameraFollow.ResetLockToTarget();
            playerCameraFollow.enabled = true;
        }

        // เปิด PlayerBounds
        if (pb != null)
            pb.enabled = true;

        // ปิด bossBounds
        if (bossBounds != null)
            bossBounds.SetActive(false);

        // กลับเพลงปกติ
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayNormalMusic();
    }
}