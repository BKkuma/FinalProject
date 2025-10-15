using UnityEngine;
using UnityEngine.UI;

public class PlayerLifeUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] energyIcons;          // ใส่ Image พลังงานทั้งหมดใน Inspector
    public Sprite fullBatterySprite;     // sprite แบตเต็ม
    public Sprite emptyBatterySprite;    // sprite แบตแตก

    void Update()
    {
        if (playerHealth == null) return;

        int livesLeft = playerHealth.autoRespawnLives - playerHealth.UsedLives;

        for (int i = 0; i < energyIcons.Length; i++)
        {
            if (i < livesLeft)
                energyIcons[i].sprite = fullBatterySprite;  // ยังเหลือชีวิต → แบตเต็ม
            else
                energyIcons[i].sprite = emptyBatterySprite; // หมดชีวิต → แบตแตก
        }
    }
}
