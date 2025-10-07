using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    [Header("Camera Settings")]
    public CameraFollow camFollow; // CameraFollow ปัจจุบัน
    private float halfWidth;

    void Start()
    {
        if (camFollow == null)
        {
            camFollow = Camera.main?.GetComponent<CameraFollow>();
            if (camFollow == null)
                Debug.LogWarning("PlayerBounds: CameraFollow ยังไม่ถูกกำหนด!");
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        halfWidth = (sr != null) ? sr.bounds.extents.x : 0.5f;
    }

    void LateUpdate()
    {
        if (camFollow == null) return;

        float leftLimit = camFollow.GetCameraLeftEdge() + halfWidth;
        float rightLimit = camFollow.GetCameraRightEdge() - halfWidth;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        transform.position = pos;
    }

    // ใช้เปลี่ยน CameraFollow ตอนสลับกล้อง
    public void SetCamera(CameraFollow newCamera)
    {
        if (newCamera != null)
            camFollow = newCamera;
    }
}
