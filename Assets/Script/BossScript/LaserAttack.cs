using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // หากใช้ Event หรือ Action

public class LaserAttack : MonoBehaviour
{
    // ⭐ MODIFIED: เปลี่ยนเป็นดาเมจคงที่ต่อการชน 1 ครั้ง
    [Header("Laser Settings")]
    [Tooltip("ค่าความเสียหายที่เลเซอร์จะทำต่อการชน 1 ครั้ง")]
    public float fixedDamage = 30f;

    [Tooltip("กำหนดให้ทำลายตัวเองทันทีเมื่อชนหรือไม่")]
    public bool destroyOnHit = true; // NEW: เพิ่มตัวแปรควบคุมการทำลายตัวเอง

    [Header("Targeting")]
    [Tooltip("Tag ของ Object ที่จะถูกเลเซอร์ทำดาเมจ (เช่น 'Player')")]
    public string targetTag = "Player";

    private bool hasHit = false; // NEW: ตัวแปรป้องกันการชนซ้ำ

    private void Start()
    {
        // หากต้องการให้เลเซอร์หายไปเองหลังจากเวลาหนึ่ง แม้ไม่ชนใครเลย
        // (ยกเลิก lifetime เดิมเพื่อใช้ logic destroyOnHit แทน)
    }

    // ⭐ MODIFIED: ใช้ OnTriggerEnter2D เพื่อดักจับการชน "ครั้งแรก"
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ป้องกันการทำดาเมจซ้ำ หาก Collider ซ้อนกันหลายเฟรม
        if (hasHit) return;

        // 🔴 ทำดาเมจ (โดยไม่ต้องคูณ Time.deltaTime)
        ApplyDamage(other.gameObject, fixedDamage);
    }

    // (ถ้าใช้ 3D)
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        ApplyDamage(other.gameObject, fixedDamage);
    }

    // ⭐ MODIFIED: รับค่าดาเมจคงที่ (float damageAmount)
    private void ApplyDamage(GameObject hitObject, float damageAmount)
    {
        // 1. ตรวจสอบ Tag ของ Object ที่ชนว่าตรงกับเป้าหมายหรือไม่
        if (hitObject.CompareTag(targetTag))
        {
            // 2. พยายามดึงสคริปต์รับดาเมจจากเป้าหมาย
            // (สมมติว่าเป้าหมายมีสคริปต์ชื่อ 'PlayerHealth')
            if (hitObject.TryGetComponent<PlayerHealth>(out PlayerHealth healthComponent))
            {
                // 3. ทำดาเมจ และตั้งค่า hasHit
                hasHit = true;

                // 4. เรียกฟังก์ชันรับดาเมจด้วยค่าดาเมจคงที่
                healthComponent.TakeDamage(damageAmount);

                // 5. ทำลายตัวเองทันทีหลังทำดาเมจ
                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}