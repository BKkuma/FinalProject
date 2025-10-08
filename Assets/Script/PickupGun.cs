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
            if (shooting != null)
            {
                switch (gunType)
                {
                    case GunType.MachineGun:
                        shooting.PickupMachineGun(ammoAmount, bulletPrefab);
                        break;
                    case GunType.Shotgun:
                        shooting.PickupShotgun(ammoAmount, bulletPrefab);
                        break;
                    case GunType.Homing:
                        shooting.PickupHomingGun(ammoAmount, bulletPrefab); // ✅ ใหม่
                        break;
                }
            }
            Destroy(gameObject);
        }
    }
}
