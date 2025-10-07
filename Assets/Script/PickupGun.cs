using UnityEngine;

public enum GunType
{
    Normal,
    MachineGun,
    Shotgun
}

public class PickupGun : MonoBehaviour
{
    public GunType gunType = GunType.MachineGun; // ประเภทปืนที่จะให้เก็บ
    public int ammoAmount = 50;                  // จำนวนกระสุน
    public GameObject bulletPrefab;              // prefab ของปืนที่จะให้เก็บ

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
                        Debug.Log("💥 เก็บปืนกลแล้ว!");
                        break;

                    case GunType.Shotgun:
                        shooting.PickupShotgun(ammoAmount, bulletPrefab);
                        Debug.Log("💥 เก็บปืนลูกซองแล้ว!");
                        break;

                    case GunType.Normal:
                        // ถ้าต้องการสามารถเพิ่มเก็บปืนปกติได้
                        break;
                }
            }

            Destroy(gameObject);
        }
    }
}
