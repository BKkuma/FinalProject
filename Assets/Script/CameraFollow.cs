using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public float minY = -2f;
    public float maxY = 5f;

    [HideInInspector] public bool bossActive = false;

    public Camera cam; // ← ลาก MainCamera จาก Inspector

    private float maxX;
    private float lockLeft;

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
        if (target == null || cam == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        if (!bossActive)
        {
            if (smoothedPosition.x > maxX) maxX = smoothedPosition.x;
            transform.position = new Vector3(Mathf.Max(maxX, lockLeft), clampedY, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(smoothedPosition.x, clampedY, transform.position.z);
        }
    }

    public float GetCameraLeftEdge()
    {
        if (cam == null) return transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x - halfWidth;
    }

    public float GetCameraRightEdge()
    {
        if (cam == null) return transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;
        return transform.position.x + halfWidth;
    }
}
