using UnityEngine;

public class PitfallTrap : MonoBehaviour
{
    public int damage = 999; // ดาเมจสูงสุด (เหมือนตายทันที)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
