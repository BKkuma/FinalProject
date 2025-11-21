using UnityEngine;
using System.Collections;
using System;

public class BossPhase2Cinematic : MonoBehaviour
{
    [Header("Boss & Object Settings")]
    public GameObject fakeBossVisual;
    public GameObject realBossObject;

    [Header("Cinematic Movement")]
    public Transform startPoint;
    public Transform endPoint;
    public float duration = 3f;
    public Vector3 initialScale = new Vector3(3, 3, 1);
    public Vector3 finalScale = new Vector3(1, 1, 1);
    public float delayBeforeFight = 1f;

    [Header("Camera Control")]
    public Camera mainCamera;
    public CameraFollowLockY playerCameraFollow;
    public Transform bossCameraPoint;           // ตำแหน่งที่กล้องจะเลื่อนไปหยุด
    public GameObject player;

    [Header("Camera Animation Settings")]
    public float camMoveDuration = 1.2f;
    public AnimationCurve camEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool hasTriggered = false;

    void Start()
    {
        if (fakeBossVisual != null) fakeBossVisual.SetActive(false);
        if (realBossObject != null) realBossObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlayIntroSequence());
        }
    }

    IEnumerator PlayIntroSequence()
    {
        // 0. ปิด Follow Script และเตรียมเลื่อนกล้อง
        if (playerCameraFollow != null)
            playerCameraFollow.enabled = false;

        // 1. Cinematic: บินและย่อส่วน
        if (fakeBossVisual != null)
        {
            float elapsed = 0f;

            // เปิดตัวปลอมและตั้งค่าเริ่มต้น
            fakeBossVisual.SetActive(true);
            fakeBossVisual.transform.position = startPoint.position;
            fakeBossVisual.transform.localScale = initialScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0, 1, t);

                fakeBossVisual.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
                fakeBossVisual.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);

                // เลื่อนกล้องตาม Cinematic
                if (mainCamera != null && bossCameraPoint != null)
                {
                    Vector3 camStart = mainCamera.transform.position;
                    Vector3 camEnd = bossCameraPoint.position;
                    camEnd.z = camStart.z;
                    mainCamera.transform.position = Vector3.Lerp(camStart, camEnd, t);
                }

                yield return null;
            }

            // ตั้งค่าสุดท้ายให้ตรงเป๊ะ
            fakeBossVisual.transform.position = endPoint.position;
            fakeBossVisual.transform.localScale = finalScale;

            if (mainCamera != null && bossCameraPoint != null)
            {
                Vector3 camEnd = bossCameraPoint.position;
                camEnd.z = mainCamera.transform.position.z;
                mainCamera.transform.position = camEnd;
            }
        }

        // 2. เอฟเฟกต์แปลงร่าง
        //  if (transformEffect != null)
        //  {
        //     Instantiate(transformEffect, endPoint.position, Quaternion.identity);
        // }

        yield return new WaitForSeconds(0.2f);

        // ** 3. ลบ Logic การล็อคขอบเขต (StartBossFight) ออก **

        // 4. สลับร่าง -> เปิดตัวจริง
        if (fakeBossVisual != null) fakeBossVisual.SetActive(false);

        if (realBossObject != null)
        {
            realBossObject.transform.position = endPoint.position;
            realBossObject.SetActive(true);
        }

        // 5. รอบอสเริ่มต่อสู้
        yield return new WaitForSeconds(delayBeforeFight);

        // 6. รอ Boss2 ตาย
        GameObject theBoss = realBossObject;
        yield return new WaitUntil(() => theBoss == null);

        // 7. **ไม่มี EndBossFight เพราะไม่ได้ StartBossFight**

        // 8. เลื่อนกล้องกลับตำแหน่งผู้เล่น
        if (player != null)
        {
            Vector3 startPos = mainCamera.transform.position;
            Vector3 playerOffset = (playerCameraFollow != null) ? playerCameraFollow.offset : Vector3.zero;
            Vector3 endPosToPlayer = player.transform.position + playerOffset;
            endPosToPlayer.z = startPos.z;

            float timer = 0f;
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
            // mainCamera.transform.position = endPosToPlayer; // ลบออก! เราจะใช้ Teleport แทน

            // 9. เปิด follow script คืน
            if (playerCameraFollow != null)
            {
                playerCameraFollow.ResetLockToTarget();

                // [FIX] ใช้ TeleportToTarget แทนการตั้งค่า position โดยตรง
                Vector3 newTargetPos = player.transform.position;
                playerCameraFollow.TeleportToTarget(newTargetPos); // กล้องจะ Clamping ตำแหน่งทันที

                playerCameraFollow.enabled = true; // เปิดการติดตามผู้เล่น
            }
        }

        // 10. จัดการสุดท้าย
        Destroy(gameObject);
    }
}