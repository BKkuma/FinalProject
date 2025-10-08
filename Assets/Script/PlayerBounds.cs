using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    [Header("Camera Settings")]
    public CameraFollowLockY camFollow; // ใช้ตัวใหม่
    private float halfWidth;

    void Start()
    {
        if (camFollow == null)
        {
            camFollow = Camera.main?.GetComponent<CameraFollowLockY>();
            if (camFollow == null)
                Debug.LogWarning("PlayerBounds: CameraFollowLockY ยังไม่ถูกกำหนด!");
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
}
