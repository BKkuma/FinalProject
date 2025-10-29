using UnityEngine;
using UnityEngine.UI;

public class PlayerLifeUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image[] heartIcons; // ช่องหัวใจแต่ละดวง

    [Header("Heart Sprites")]
    public Sprite heartFull;     // หัวใจปกติ
    public Sprite heartBroken;   // หัวใจแตก (เสียแล้ว)

    void Update()
    {
        if (playerHealth == null) return;

        int totalLives = playerHealth.autoRespawnLives;
        int usedLives = playerHealth.UsedLives;   // จำนวนหัวใจที่เสียไปแล้ว
        int livesLeft = totalLives - usedLives;   // จำนวนหัวใจที่เหลืออยู่

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (i < livesLeft)
            {
                // ยังมีชีวิต → หัวใจเต็ม
                heartIcons[i].sprite = heartFull;
            }
            else
            {
                // หัวใจที่เสียไป → หัวใจแตก
                heartIcons[i].sprite = heartBroken;
            }

            // ให้ช่องทั้งหมดแสดงเท่าจำนวนหัวใจสูงสุดเท่านั้น
            heartIcons[i].enabled = (i < totalLives);
        }
    }
}
