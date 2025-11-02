using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("References")]
    public WeaponSlotManager slotManager;
    public PlayerShooting playerShooting;

    [Header("UI Components")]
    public Image backgroundImage;   // BG คงที่ (ตั้งใน Inspector)
    public Image weaponArtImage;    // รูปปืนที่จะเปลี่ยน
               // แสดงจำนวนกระสุน

    [Header("Weapon Art Sprites")]
    public Sprite normalGunArt;
    public Sprite machineGunArt;
    public Sprite shotgunArt;
    public Sprite homingGunArt;

    private int lastSlot = -1;

    void Update()
    {
        if (slotManager == null || playerShooting == null) return;

        int currentSlot = slotManager.currentSlot;
        var slot = slotManager.slots[currentSlot];

        // ✅ อัปเดต art เฉพาะตอนเปลี่ยนปืน
        if (currentSlot != lastSlot)
        {
            UpdateWeaponArt(slot.weaponName);
            lastSlot = currentSlot;
        }

        // 🎯 อัปเดตจำนวนกระสุน
        string ammoDisplay = "∞";
        if (slot.weaponName == "MachineGun")
            ammoDisplay = playerShooting.MachineGunAmmo.ToString();
        else if (slot.weaponName == "Shotgun")
            ammoDisplay = playerShooting.ShotgunAmmo.ToString();
        else if (slot.weaponName == "HomingGun")
            ammoDisplay = playerShooting.HomingAmmo.ToString();

        
    }

    void UpdateWeaponArt(string weaponName)
    {
        Sprite newArt = null;

        switch (weaponName)
        {
            case "MachineGun": newArt = machineGunArt; break;
            case "Shotgun": newArt = shotgunArt; break;
            case "HomingGun": newArt = homingGunArt; break;
            default: newArt = normalGunArt; break;
        }

        if (weaponArtImage != null)
            weaponArtImage.sprite = newArt;
    }
}
