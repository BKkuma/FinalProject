using UnityEngine;

public class WeaponSlotManager : MonoBehaviour
{
    public WeaponSlot[] slots; // 0 = NormalGun, 1-3 = special weapons
    public int currentSlot = 0;
    public PlayerShooting playerShooting;

    void Start() => ApplySlotToShooting(); // แก้ไข: ลบ (false) ออก

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
        ApplySlotToShooting(); // แก้ไข: ลบ (false) ออก
    }

    public void AddWeaponToSlot(string weaponName, int ammo, GameObject bulletPrefab)
    {
        // เพิ่ม ammo ถ้ามี slot เดิม (และ WeaponSlotManager จะอัปเดตค่า ammo ใน PlayerShooting โดยตรง)
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

                // ถ้า slot นี้เป็น currentSlot ให้ switch ไปใช้ปืนนี้ทันที
                if (i == currentSlot)
                {
                    switch (weaponName)
                    {
                        case "MachineGun":
                            // แก้ไข: ลบ Argument ที่สอง (ammo)
                            playerShooting.SwitchToMachineGun(bulletPrefab);
                            break;
                        case "Shotgun":
                            // แก้ไข: ลบ Argument ที่สอง (ammo)
                            playerShooting.SwitchToShotgun(bulletPrefab);
                            break;
                        case "HomingGun":
                            // แก้ไข: ลบ Argument ที่สอง (ammo)
                            playerShooting.SwitchToHomingGun(bulletPrefab);
                            break;
                    }
                }
                return;
            }
        }
    }


    void ApplySlotToShooting() // แก้ไข: ลบ Argument 'bool addAmmo'
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
                // แก้ไข: ลบ Argument ที่สอง
                playerShooting.SwitchToMachineGun(slot.bulletPrefab);
                break;
            case "Shotgun":
                // แก้ไข: ลบ Argument ที่สอง
                playerShooting.SwitchToShotgun(slot.bulletPrefab);
                break;
            case "HomingGun":
                // แก้ไข: ลบ Argument ที่สอง
                playerShooting.SwitchToHomingGun(slot.bulletPrefab);
                break;
        }
    }
}