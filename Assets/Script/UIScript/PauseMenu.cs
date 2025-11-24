using UnityEngine;
using UnityEngine.SceneManagement; // ⭐ จำเป็นสำหรับการโหลด Scene

public class PauseMenu : MonoBehaviour
{
    // ⭐ [ตั้งค่าใน Inspector]: ใส่ชื่อ Scene ของเมนูหลักที่นี่
    // ผมเปลี่ยนค่าเริ่มต้นเป็น "MenuScene" ตามคำขอของคุณ แต่คุณสามารถปรับใน Inspector ได้
    public string menuSceneName = "MenuScene";

    // ลบตัวแปร isPaused ออก

    void Update()
    {
        // ตรวจสอบว่าผู้เล่นกดปุ่ม Escape หรือไม่
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // เรียกฟังก์ชันโหลด Scene เมนูหลักทันที
            GoToMenu();
        }
    }

    // ลบฟังก์ชัน Pause() ออก

    // ลบฟังก์ชัน Resume() ออก

    // ฟังก์ชันสำหรับกลับไปยังหน้าเมนูหลัก
    public void GoToMenu()
    {
        // 1. ตรวจสอบชื่อ Scene
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            // 2. คืนค่า Time.timeScale ให้เป็น 1f ก่อนโหลด Scene ใหม่
            // เพื่อให้แน่ใจว่า Scene เมนูจะทำงานตามปกติ ถึงแม้ว่าเกมก่อนหน้าจะหยุดเวลาไว้ก็ตาม
            Time.timeScale = 1f;

            // 3. โหลด Scene เมนูหลัก
            if (SceneManager.GetActiveScene().name != menuSceneName)
            {
                SceneManager.LoadScene(menuSceneName);
                Debug.Log($"Loading Menu Scene: {menuSceneName}");
            }
            else
            {
                Debug.Log("Already in the menu scene. Doing nothing.");
            }
        }
        else
        {
            Debug.LogError("Menu Scene Name is not set in the Inspector! Please set the Scene name.");
        }
    }
}