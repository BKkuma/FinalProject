using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    public WeaponSlotManager slotManager;
    public PlayerShooting shooting;

    public Image weaponArtImage;

    public Sprite normalGunArt;
    public Sprite machineGunArt;
    public Sprite shotgunArt;
    public Sprite homingGunArt;

    private int lastSlot = -1;

    void Update()
    {
        if (slotManager == null || shooting == null) return;

        int slotID = slotManager.currentSlot;
        var slot = slotManager.slots[slotID];

        if (lastSlot != slotID)
        {
            UpdateWeaponArt(slot.weaponName);
            lastSlot = slotID;
        }
    }

    void UpdateWeaponArt(string weaponName)
    {
        switch (weaponName)
        {
            case "MachineGun": weaponArtImage.sprite = machineGunArt; break;
            case "Shotgun": weaponArtImage.sprite = shotgunArt; break;
            case "HomingGun": weaponArtImage.sprite = homingGunArt; break;
            default: weaponArtImage.sprite = normalGunArt; break;
        }
    }
}
