using UnityEngine;

public class PickupGun : MonoBehaviour
{
    public int ammoAmount = 50; // จำนวนกระสุน
    public GameObject machineGunBulletPrefab; // prefab ของปืนกล

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShooting shooting = other.GetComponent<PlayerShooting>();
            if (shooting != null)
            {
                // เรียก PickupMachineGun ส่ง ammo + prefab
                shooting.PickupMachineGun(ammoAmount, machineGunBulletPrefab);
                Debug.Log("💥 เก็บปืนกลแล้ว!");
            }

            Destroy(gameObject);
        }
    }
}
