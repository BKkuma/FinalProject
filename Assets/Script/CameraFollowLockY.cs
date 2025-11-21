using UnityEngine;

public class CameraFollowLockY : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    [Header("Lock Y Settings")]
    public float minY = 4f;
    public float maxY = 5f;

    [Header("Boss Lock")]
    public bool bossActive = false;
    public Vector2 bossMinLimit;
    public Vector2 bossMaxLimit;

    private float maxX;
    private float lockLeft;
    private bool inBossFight = false;

    [Header("Camera X Clamp Limit")]
    public float cameraRightLimit = Mathf.Infinity;

    void Start()
    {
        if (target != null)
        {
            maxX = target.position.x + offset.x;
            lockLeft = target.position.x + offset.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        float clampedY = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        if (inBossFight)
        {
            float clampX = Mathf.Clamp(smoothedPosition.x, bossMinLimit.x, bossMaxLimit.x);
            float clampY = Mathf.Clamp(smoothedPosition.y, bossMinLimit.y, bossMaxLimit.y);
            transform.position = new Vector3(clampX, clampY, transform.position.z);
        }
        else
        {
            if (smoothedPosition.x > maxX) maxX = smoothedPosition.x;
            transform.position = new Vector3(Mathf.Max(maxX, lockLeft), clampedY, transform.position.z);
        }
    }

    public void StartBossFight(Vector2 min, Vector2 max)
    {
        inBossFight = true;
        bossMinLimit = min;
        bossMaxLimit = max;
    }

    public void EndBossFight()
    {
        inBossFight = false;
    }

    // --- ส่วนที่เพิ่มกลับมาเพื่อให้ PlayerBounds ทำงานได้ ---
    public float GetCameraLeftEdge()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return transform.position.x;

        // คำนวณขอบซ้ายจาก Orthographic Size และ Aspect Ratio
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x - halfWidth;
    }

    public float GetCameraRightEdge()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return transform.position.x;

        // คำนวณขอบขวา
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x + halfWidth;
    }
    // --------------------------------------------------

    // ฟังก์ชันที่ใช้ในการย้ายกล้องทันที (สำคัญในการแก้ไขปัญหานี้)
    public void TeleportToTarget(Vector3 newTargetPos)
    {
        Vector3 desiredPosition = newTargetPos + offset;
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        if (inBossFight)
        {
            float clampX = Mathf.Clamp(desiredPosition.x, bossMinLimit.x, bossMaxLimit.x);
            float clampY = Mathf.Clamp(desiredPosition.y, bossMinLimit.y, bossMaxLimit.y);
            transform.position = new Vector3(clampX, clampY, transform.position.z);
        }
        else
        {
            if (desiredPosition.x > maxX) maxX = desiredPosition.x;
            transform.position = new Vector3(Mathf.Max(maxX, lockLeft), clampedY, transform.position.z);
        }
    }

    // ฟังก์ชันสำหรับรีเซ็ตกล้องหลังจบบอส (แก้ไขเพื่อรีเซ็ต lockLeft ด้วย)
    public void ResetLockToTarget()
    {
        if (target != null)
        {
            // [FIX] รีเซ็ต maxX และ lockLeft ให้เท่ากับตำแหน่งผู้เล่นปัจจุบัน
            maxX = target.position.x + offset.x;
            lockLeft = target.position.x + offset.x;
        }
    }
}