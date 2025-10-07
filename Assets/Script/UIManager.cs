using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject winCanvas; // ใส่ทั้ง Canvas ที่มี Text + Button

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (winCanvas != null)
            winCanvas.SetActive(false); // ปิดก่อนเริ่มเกม
    }

    public void ShowWinUI()
    {
        if (winCanvas != null)
        {
            winCanvas.SetActive(true); // เปิด Canvas
        }
        Time.timeScale = 0f; // หยุดเกม
    }

    public void OnPlayAgain()
    {
        Time.timeScale = 1f; // รีเซ็ตเวลา
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // โหลด Scene ปัจจุบัน
    }
}
