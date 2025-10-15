using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public PlayerShooting playerShooting;

    [Header("UI References")]
    public Image ammoIcon;     // ✅ รูปกระสุน
    public Text ammoText;      // ✅ จำนวนกระสุน

    [Header("Ammo Sprites")]
    public Sprite normalAmmoSprite;
    public Sprite machineGunSprite;
    public Sprite shotgunSprite;
    public Sprite homingSprite;

    void Update()
    {
        if (playerShooting == null || ammoText == null || ammoIcon == null) return;

        int ammoCount = -1;

        // เปลี่ยนไอคอนและจำนวนกระสุนตามปืน
        if (playerShooting.IsUsingMachineGun)
        {
            ammoIcon.sprite = machineGunSprite;
            ammoCount = playerShooting.MachineGunAmmo;
        }
        else if (playerShooting.IsUsingShotgun)
        {
            ammoIcon.sprite = shotgunSprite;
            ammoCount = playerShooting.ShotgunAmmo;
        }
        else if (playerShooting.IsUsingHoming)
        {
            ammoIcon.sprite = homingSprite;
            ammoCount = playerShooting.HomingAmmo;
        }
        else
        {
            ammoIcon.sprite = normalAmmoSprite;
        }

        // แสดงจำนวนกระสุน
        if (ammoCount < 0)
            ammoText.text = "∞";  // ปืนธรรมดา
        else
            ammoText.text = ammoCount.ToString();
    }
}
