using UnityEngine;

public class CameraFollowLockY : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    [Header("Lock Y Settings")]
    public float minY = -2f;   // ล็อกสูงต่ำ
    public float maxY = 5f;

    [Header("Boss Lock")]
    public bool bossActive = false;
    public Vector2 bossMinLimit;
    public Vector2 bossMaxLimit;

    private float maxX;
    private float lockLeft;
    private bool inBossFight = false;

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
            // Boss fight lock
            float clampX = Mathf.Clamp(smoothedPosition.x, bossMinLimit.x, bossMaxLimit.x);
            float clampY = Mathf.Clamp(smoothedPosition.y, bossMinLimit.y, bossMaxLimit.y);
            transform.position = new Vector3(clampX, clampY, transform.position.z);
        }
        else
        {
            // ปกติ
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

    // สำหรับ PlayerBounds
    public float GetCameraLeftEdge()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x - halfWidth;
    }

    public float GetCameraRightEdge()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x + halfWidth;
    }
}
