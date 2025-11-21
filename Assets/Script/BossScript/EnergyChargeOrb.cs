using UnityEngine;
using System.Collections;
using System; // ใช้สำหรับ Action

public class EnergyChargeOrb : MonoBehaviour
{
    [Header("Charge Settings")]
    public float chargeDuration = 2.0f;   // เวลาในการชาร์จ (วินาที)
    public Vector3 targetScale = new Vector3(2.5f, 2.5f, 1f); // ขนาดสูงสุดของลูกพลัง
    public float rotateSpeed = 360f;      // ความเร็วการหมุน (องศา/วินาที)

    [Header("Visuals & Effects")]
    public SpriteRenderer spriteRenderer; // ลาก Sprite Renderer ของ Orb มาใส่
    public GameObject impactEffect;       // เอฟเฟกต์ระเบิดตอนชาร์จเสร็จ (Optional)

    // ตัวแปรสำหรับแจ้งเตือนบอสเมื่อชาร์จเสร็จ
    public Action OnChargeComplete;

    void Start()
    {
        // เริ่มต้นขนาดจากศูนย์
        transform.localScale = Vector3.zero;

        // ตั้งให้ลูกพลังอยู่ด้านหลังบอส
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = -1;
        }

        StartCoroutine(ChargeRoutine());
    }

    void Update()
    {
        // หมุนตลอดเวลา
        transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
    }

    IEnumerator ChargeRoutine()
    {
        float timer = 0f;

        while (timer < chargeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / chargeDuration;

            // ทำให้การขยายดูนุ่มนวล
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // ขยายขนาด
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, smoothProgress);

            yield return null;
        }

        // เมื่อชาร์จเสร็จ
        FinishCharging();
    }

    void FinishCharging()
    {
        // 1. เล่น Effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // 2. แจ้งเตือนบอสว่าพร้อมเปลี่ยน Phase แล้ว
        OnChargeComplete?.Invoke();

        // 3. ทำลายลูกพลัง
        Destroy(gameObject);
    }
}