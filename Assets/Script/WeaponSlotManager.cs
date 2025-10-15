using UnityEngine;

public class WeaponSlotManager : MonoBehaviour
{
    private PlayerShooting shooting;

    [System.Serializable]
    public class WeaponSlot
    {
        public string weaponName;
        public bool isUnlocked;
        public int ammo;
        public GameObject bulletPrefab;
        public Sprite weaponIcon;
    }

    [Header("Weapon Slots (1 = Default, 2–4 = Pickup Guns)")]
    public WeaponSlot[] slots = new WeaponSlot[4];

    public int currentSlot = 0;

    void Start()
    {
        shooting = GetComponent<PlayerShooting>();
        // ตั้งค่า slot 0 ให้เป็นปืนปกติ
        slots[0].isUnlocked = true;
        slots[0].weaponName = "NormalGun";
        slots[0].bulletPrefab = shooting.normalBulletPrefab;
    }

    void Update()
    {
        HandleSlotSwitch();
    }

    void HandleSlotSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(3);
    }

    public void SwitchWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        // 🟡 ก่อนสลับปืน เก็บค่ากระสุนของปืนปัจจุบัน
        UpdateCurrentSlotAmmo(shooting);

        if (!slots[slotIndex].isUnlocked)
        {
            Debug.Log($"❌ ยังไม่มีปืนในช่อง {slotIndex + 1}");
            return;
        }

        currentSlot = slotIndex;
        ApplySlotToShooting();
        Debug.Log($"🔫 เปลี่ยนเป็น {slots[slotIndex].weaponName}");
    }


    void ApplySlotToShooting()
    {
        var slot = slots[currentSlot];
        shooting.ResetToNormalGun(); // รีเซ็ตสถานะก่อน

        switch (slot.weaponName)
        {
            case "MachineGun":
                shooting.PickupMachineGun(slot.ammo, slot.bulletPrefab);
                break;
            case "Shotgun":
                shooting.PickupShotgun(slot.ammo, slot.bulletPrefab);
                break;
            case "HomingGun":
                shooting.PickupHomingGun(slot.ammo, slot.bulletPrefab);
                break;
            default:
                shooting.ResetToNormalGun();
                break;
        }
    }

    // ใช้เรียกจาก PickupGun
    public void AddWeaponToSlot(string weaponName, int ammo, GameObject bulletPrefab)
    {
        for (int i = 1; i < slots.Length; i++) // เริ่มที่ช่อง 2
        {
            if (!slots[i].isUnlocked)
            {
                slots[i].isUnlocked = true;
                slots[i].weaponName = weaponName;
                slots[i].ammo = ammo;
                slots[i].bulletPrefab = bulletPrefab;
                Debug.Log($"✅ ได้ {weaponName} เข้า Slot {i + 1}");
                return;
            }
        }

        Debug.Log("⚠️ ไม่มีช่องว่างสำหรับปืนใหม่!");
    }

    public void RemoveWeaponFromCurrentSlot()
    {
        slots[currentSlot] = new WeaponSlot(); // reset ช่องนี้
        SwitchWeapon(0); // กลับไปใช้ปืนเริ่มต้น
    }


    public void UpdateCurrentSlotAmmo(PlayerShooting shooting)
    {
        var slot = slots[currentSlot];

        if (slot.weaponName == "MachineGun")
            slot.ammo = shooting.MachineGunAmmo;
        else if (slot.weaponName == "Shotgun")
            slot.ammo = shooting.ShotgunAmmo;
        else if (slot.weaponName == "HomingGun")
            slot.ammo = shooting.HomingAmmo;
    }

}
