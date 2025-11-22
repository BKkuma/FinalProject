using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonManager : MonoBehaviour
{
    // เรียกจากปุ่ม Start
    public void StartGame()
    {
        // เปลี่ยนชื่อ "GameScene" เป็นชื่อจริงของ Scene gameplay ของคุณ
        SceneManager.LoadScene("MapLv1");
    }

    // ⭐ NEW: เรียกจากปุ่ม Credit
    public void LoadCredits()
    {
        // ต้องแน่ใจว่าได้สร้าง Scene ชื่อ "CreditScene" แล้ว
        SceneManager.LoadScene("CreditScene");
    }

    // เรียกจากปุ่ม Exit
    public void ExitGame()
    {
        Debug.Log("Quit Game"); // สำหรับทดสอบใน Editor
        Application.Quit();      // ใช้งานจริงจะปิดเกม
    }
}