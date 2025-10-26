using UnityEngine;
using UnityEngine.UI;

public class PlayerLifeUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Image[] heartIcons; // ช่องหัวใจแต่ละดวง

    [Header("Heart Sprites")]
    public Sprite heartFull3;   // หัวใจเต็ม 3 หลอด (ชีวิตสมบูรณ์)
    public Sprite heartFull2;   // เหลือ 2 หลอด (ยังพอไหว)
    public Sprite heartFull1;   // เหลือ 1 หลอด (ใกล้ตาย)
    public Sprite heartEmpty;   // หมดพลัง แต่ยังไม่แตก
    public Sprite heartBroken;  // หัวใจแตก (ชีวิตหมด)

    void Update()
    {
        if (playerHealth == null) return;

        int totalLives = playerHealth.autoRespawnLives;
        int usedLives = playerHealth.UsedLives;
        int livesLeft = totalLives - usedLives;

        bool isGameOver = livesLeft <= 0 && playerHealth.gameOverUI.activeSelf;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (i < livesLeft)
            {
                // หัวใจยังมีชีวิต → แสดงตามจำนวนหลอด
                int remaining = livesLeft - i;

                if (remaining >= 3)
                    heartIcons[i].sprite = heartFull3;
                else if (remaining == 2)
                    heartIcons[i].sprite = heartFull2;
                else if (remaining == 1)
                    heartIcons[i].sprite = heartFull1;
            }
            else
            {
                // หมดหัวใจดวงนี้
                if (isGameOver)
                    heartIcons[i].sprite = heartBroken; // แสดงตอน GameOver
                else
                    heartIcons[i].sprite = heartEmpty;  // แสดงว่าง
            }
        }
    }

}
