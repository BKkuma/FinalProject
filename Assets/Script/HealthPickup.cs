using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    public int livesToRestore = 1;

    [Header("Effect & Sound")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSFX;

    

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
               
                if (playerHealth.RestoreLives(livesToRestore))
                {
                    
                    if (pickupEffectPrefab != null)
                    {
                        
                        Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                    }
                    if (pickupSFX != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSFX, transform.position);
                    }

                   
                    Destroy(gameObject);
                }
            }
        }
    }
}