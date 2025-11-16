using UnityEngine;

public class WeaponSlotManager : MonoBehaviour
{
    public WeaponSlot[] slots; // 0 = NormalGun, 1-3 = special weapons
    public int currentSlot = 0;
    public PlayerShooting playerShooting;

    void Start() => ApplySlotToShooting(false);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSlot(3);
    }

    void ChangeSlot(int newSlot)
    {
        if (newSlot < 0 || newSlot >= slots.Length) return;
        currentSlot = newSlot;
        ApplySlotToShooting(false); // แค่สลับปืน ไม่เติม ammo
    }

    public void AddWeaponToSlot(string weaponName, int ammo, GameObject bulletPrefab)
    {
        // เพิ่ม ammo ถ้ามี slot เดิม
        for (int i = 1; i < slots.Length; i++)
        {
            if (slots[i].isUnlocked && slots[i].weaponName == weaponName)
            {
                slots[i].ammo += ammo;

                // อัป ammo ของ PlayerShooting ทุกครั้ง
                switch (weaponName)
                {
                    case "MachineGun":
                        playerShooting.machineGunAmmo += ammo;
                        break;
                    case "Shotgun":
                        playerShooting.shotgunAmmo += ammo;
                        break;
                    case "HomingGun":
                        playerShooting.homingAmmo += ammo;
                        break;
                }

                return;
            }
        }

        // หา slot ว่าง เพิ่มปืนใหม่
        for (int i = 1; i < slots.Length; i++)
        {
            if (!slots[i].isUnlocked)
            {
                slots[i].isUnlocked = true;
                slots[i].weaponName = weaponName;
                slots[i].ammo = ammo;
                slots[i].bulletPrefab = bulletPrefab;

                // ถ้า slot นี้เป็น currentSlot ให้ switch ammo ให้ PlayerShooting
                if (i == currentSlot)
                {
                    switch (weaponName)
                    {
                        case "MachineGun":
                            playerShooting.SwitchToMachineGun(bulletPrefab, ammo);
                            break;
                        case "Shotgun":
                            playerShooting.SwitchToShotgun(bulletPrefab, ammo);
                            break;
                        case "HomingGun":
                            playerShooting.SwitchToHomingGun(bulletPrefab, ammo);
                            break;
                    }
                }
                return;
            }
        }
    }



    void ApplySlotToShooting(bool addAmmo = true)
    {
        var slot = slots[currentSlot];

        if (!slot.isUnlocked)
        {
            playerShooting.SwitchToNormalGun();
            return;
        }

        switch (slot.weaponName)
        {
            case "MachineGun":
                playerShooting.SwitchToMachineGun(slot.bulletPrefab, addAmmo ? slot.ammo : 0);
                break;
            case "Shotgun":
                playerShooting.SwitchToShotgun(slot.bulletPrefab, addAmmo ? slot.ammo : 0);
                break;
            case "HomingGun":
                playerShooting.SwitchToHomingGun(slot.bulletPrefab, addAmmo ? slot.ammo : 0);
                break;
        }
    }
}
