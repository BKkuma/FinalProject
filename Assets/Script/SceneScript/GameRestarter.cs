using UnityEngine;
using UnityEngine.SceneManagement; // 👈 ต้องเพิ่ม namespace นี้

public class GameRestarter : MonoBehaviour
{
    void Update()
    {
        // ตรวจสอบว่าผู้เล่นกดปุ่ม 'R' หรือไม่
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        // โหลดฉากปัจจุบันซ้ำอีกครั้ง
        // SceneManager.GetActiveScene().buildIndex จะคืนค่า Index ของฉากที่เรากำลังเล่นอยู่
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // สำหรับคนที่ใช้ Time.timeScale เมื่อ Boss ตาย:
        // ตรวจสอบให้แน่ใจว่าเวลาเดินตามปกติ หากคุณตั้ง Time.timeScale = 0 ไว้ก่อนหน้านี้
        Time.timeScale = 1f;

        Debug.Log("Game Restarted (Scene Reloaded) by R key.");
    }
}