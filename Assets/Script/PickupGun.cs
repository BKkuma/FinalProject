using UnityEngine;

public class PickupGun : MonoBehaviour
{
    public enum GunType { MachineGun, Shotgun, Homing } // ✅ เพิ่ม Homing
    public GunType gunType;

    public int ammoAmount = 50;
    public GameObject bulletPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShooting shooting = other.GetComponent<PlayerShooting>();
            WeaponSlotManager slotManager = other.GetComponent<WeaponSlotManager>();

            if (shooting != null && slotManager != null)
            {
                switch (gunType)
                {
                    case GunType.MachineGun:
                        slotManager.AddWeaponToSlot("MachineGun", ammoAmount, bulletPrefab);
                        break;
                    case GunType.Shotgun:
                        slotManager.AddWeaponToSlot("Shotgun", ammoAmount, bulletPrefab);
                        break;
                    case GunType.Homing:
                        slotManager.AddWeaponToSlot("HomingGun", ammoAmount, bulletPrefab);
                        break;
                }
            }

            Destroy(gameObject);
        }
    }

}
