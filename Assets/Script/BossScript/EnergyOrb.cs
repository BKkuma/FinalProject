using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    public Animator bossAnimator;
    public string powerUpTrigger = "PowerUp";
    public GameObject impactEffect; // เอฟเฟกต์ตอนโดน (optional)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) // อย่าลืมตั้ง tag ให้บอส
        {
            // เอฟเฟกต์ตอนลูกพลังชน
            if (impactEffect)
                Instantiate(impactEffect, collision.transform.position, Quaternion.identity);

            // เปลี่ยนร่างบอส
            bossAnimator.SetTrigger(powerUpTrigger);

            // ลบลูกพลังออก
            Destroy(gameObject);
        }
    }
}
