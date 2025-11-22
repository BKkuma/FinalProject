using UnityEngine;

public class PickupGun : MonoBehaviour
{
    public enum GunType { MachineGun, Shotgun, Homing }
    public GunType gunType;

    public int ammoAmount = 50;
    public GameObject bulletPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerShooting playerShooting = other.GetComponent<PlayerShooting>();
        WeaponSlotManager slotManager = other.GetComponent<WeaponSlotManager>();

        if (playerShooting == null && slotManager == null) return;

        string gunName = "";
        switch (gunType)
        {
            case GunType.MachineGun:
                gunName = "MachineGun";
                break;
            case GunType.Shotgun:
                gunName = "Shotgun";
                break;
            case GunType.Homing:
                gunName = "HomingGun";
                break;
        }

        // 1. เพิ่มกระสุนและเล่นเสียงเก็บปืน/เสียงพูด (สำคัญ: เรียก PickupAmmo)
        if (playerShooting != null)
        {
            playerShooting.PickupAmmo(gunName, ammoAmount);
        }

        // 2. หากมี WeaponSlotManager: 
        if (slotManager != null)
        {
            // Note: ถ้าคุณใช้ slotManager คุณอาจต้องแน่ใจว่า slotManager 
            // ไม่ได้เรียก PlayerShooting.SwitchTo...Gun() ซ้ำ
            slotManager.AddWeaponToSlot(gunName, ammoAmount, bulletPrefab);
        }

        Destroy(gameObject);
    }
}