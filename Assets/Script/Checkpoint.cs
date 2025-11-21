using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจสอบว่าชนกับ Player และยังไม่เคยเปิดใช้งาน
        if (other.CompareTag("Player") && !isActivated)
        {
            // เรียก Manager เพื่อบันทึกตำแหน่งของ Checkpoint นี้
            CheckpointManager.SetCheckpoint(transform.position);

            isActivated = true;

            // **[Optional]** หากต้องการให้ Checkpoint มีภาพเปลี่ยนไปเมื่อถูกใช้งาน
            // ตัวอย่าง: ถ้ามี SpriteRenderer ให้เปลี่ยนสีเป็นสีเขียว
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.green; // เปลี่ยนสีเป็นสีเขียว
            }
        }
    }
}