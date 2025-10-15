using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    public WeaponSlotManager slotManager;
    public PlayerShooting playerShooting;
    public Text[] slotTexts;              // ใช้ Text อย่างเดียวแทน icon
    public Color activeColor = Color.yellow;
    public Color inactiveColor = Color.white;

    void Update()
    {
        if (slotManager == null || slotTexts.Length == 0) return;

        for (int i = 0; i < slotTexts.Length; i++)
        {
            var slot = slotManager.slots[i];
            bool isActive = (i == slotManager.currentSlot);

            // เปลี่ยนสีช่องที่เลือกอยู่
            slotTexts[i].color = isActive ? activeColor : inactiveColor;

            if (slot.isUnlocked)
            {
                string ammoText = "∞";

                if (slot.weaponName == "MachineGun")
                    ammoText = playerShooting.MachineGunAmmo.ToString();
                else if (slot.weaponName == "Shotgun")
                    ammoText = playerShooting.ShotgunAmmo.ToString();
                else if (slot.weaponName == "HomingGun")
                    ammoText = playerShooting.HomingAmmo.ToString();

                // แสดงเป็นเลขช่อง + ชื่อ + กระสุน
                slotTexts[i].text = $"{i + 1}. {slot.weaponName}\nAmmo: {ammoText}";
            }
            else
            {
                slotTexts[i].text = $"{i + 1}. Empty";
            }
        }
    }
}
