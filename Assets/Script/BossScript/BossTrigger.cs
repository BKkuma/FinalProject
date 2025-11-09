using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Camera Settings")]
    public CameraBossLock cameraLock;    // อ้างอิงกล้อง
    public Vector2 cameraMinLimit;       // ขอบซ้าย-ล่าง
    public Vector2 cameraMaxLimit;       // ขอบขวา-บน

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // Spawn บอส
            Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

            // Lock กล้อง
            if (cameraLock != null)
            {
                cameraLock.StartBossFight(cameraMinLimit, cameraMaxLimit);
            }

            // ปิด Trigger
            gameObject.SetActive(false);
        }
    }
}
