using UnityEngine;

public class CameraBossLock : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    [HideInInspector] public bool lockBossRoom = false;
    [HideInInspector] public Vector2 minLimit;
    [HideInInspector] public Vector2 maxLimit;

    private bool inBossFight = false;
    public CameraFollow playerCamera;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        if (inBossFight && lockBossRoom)
        {
            float clampX = Mathf.Clamp(smoothedPosition.x, minLimit.x, maxLimit.x);
            float clampY = Mathf.Clamp(smoothedPosition.y, minLimit.y, maxLimit.y);
            transform.position = new Vector3(clampX, clampY, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
        }
    }

    public void StartBossFight(Vector2 min, Vector2 max)
    {
        inBossFight = true;
        lockBossRoom = true;
        minLimit = min;
        maxLimit = max;

        if (playerCamera != null)
            playerCamera.enabled = false; // ปิด CameraFollow ชั่วคราว
    }


    public void EndBossFight()
    {
        inBossFight = false;
        lockBossRoom = false;

        if (playerCamera != null)
            playerCamera.enabled = true; // กล้องกลับมาตาม Player
    }
}
