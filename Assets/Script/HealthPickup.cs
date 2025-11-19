using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    public int livesToRestore = 1;

    [Header("Effect & Sound")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSFX;

    // ไม่จำเป็นต้องมี private AudioSource audioSource; ในสคริปต์แล้ว เพราะใช้ PlayClipAtPoint

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจสอบว่าชนกับผู้เล่นหรือไม่
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // **1. พยายามฟื้นฟูชีวิต**
                if (playerHealth.RestoreLives(livesToRestore))
                {
                    // **2. เล่น Effect และเสียง (ถ้ามี)**
                    if (pickupEffectPrefab != null)
                    {
                        // สร้าง Effect
                        Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                    }
                    if (pickupSFX != null)
                    {
                        // เล่นเสียงแบบ OneShot ที่ตำแหน่งของ Object นี้ (ไม่ขึ้นกับ AudioSource ใน Object)
                        AudioSource.PlayClipAtPoint(pickupSFX, transform.position);
                    }

                    // **3. ทำลายตัวเองเมื่อเก็บสำเร็จ**
                    Destroy(gameObject);
                }
            }
        }
    }
}