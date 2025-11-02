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

    // เรียกจากปุ่ม Exit
    public void ExitGame()
    {
        Debug.Log("Quit Game"); // สำหรับทดสอบใน Editor
        Application.Quit();      // ใช้งานจริงจะปิดเกม
    }
}
