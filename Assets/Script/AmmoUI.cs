using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public PlayerShooting playerShooting;

    [Header("UI References")]
    public Image[] normalAmmoIcons;
    public Image[] machineGunAmmoIcons;
    public Image[] shotgunAmmoIcons;
    public Image[] homingAmmoIcons;
    public Text ammoText;

    void Update()
    {
        if (playerShooting == null) return;

        // ปิดทุก Image ก่อน
        SetIconsActive(normalAmmoIcons, 0);
        SetIconsActive(machineGunAmmoIcons, 0);
        SetIconsActive(shotgunAmmoIcons, 0);
        SetIconsActive(homingAmmoIcons, 0);

        // แสดงปืนตามชนิด
        if (playerShooting.IsUsingMachineGun)
        {
            SetIconsActive(machineGunAmmoIcons, playerShooting.MachineGunAmmo);
            ammoText.text = playerShooting.MachineGunAmmo.ToString();
        }
        else if (playerShooting.IsUsingShotgun)
        {
            SetIconsActive(shotgunAmmoIcons, playerShooting.ShotgunAmmo);
            ammoText.text = playerShooting.ShotgunAmmo.ToString();
        }
        else if (playerShooting.IsUsingHoming)
        {
            SetIconsActive(homingAmmoIcons, playerShooting.HomingAmmo);
            ammoText.text = playerShooting.HomingAmmo.ToString();
        }
        else
        {
            // ปืนธรรมดา
            ammoText.text = "∞";
            SetIconsActive(normalAmmoIcons, normalAmmoIcons.Length); // แสดงไอคอนเต็ม
        }
    }

    void SetIconsActive(Image[] icons, int count)
    {
        if (icons == null) return;
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].gameObject.SetActive(i < count);
        }
    }
}
