using UnityEngine;

// Script นี้จะเก็บตำแหน่งล่าสุดที่ผู้เล่นควรจะ Respawn
public class CheckpointManager : MonoBehaviour
{
    // 🎯 ตัวแปร Static ที่เก็บตำแหน่ง Respawn ล่าสุด
    public static Vector3 RespawnPoint;

    // 💡 ตำแหน่งเริ่มต้นของฉาก (ตั้งค่าใน Editor)
    [SerializeField] private Transform startingSpawnPoint;

    void Awake()
    {
        // 1. ตั้งค่า Respawn Point เริ่มต้นเมื่อฉากโหลด
        if (startingSpawnPoint != null)
        {
            RespawnPoint = startingSpawnPoint.position;
        }
        else
        {
            // ถ้าไม่ได้ตั้งค่า startingSpawnPoint ให้ใช้ตำแหน่งของ Manager นี้แทน
            RespawnPoint = transform.position;
        }
    }

    // 2. ฟังก์ชันสาธารณะสำหรับ Checkpoint ในการเรียกเพื่ออัปเดตตำแหน่ง
    public static void SetCheckpoint(Vector3 newPosition)
    {
        RespawnPoint = newPosition;
        Debug.Log("Checkpoint ถูกตั้งค่าที่: " + newPosition);
    }
}