using UnityEngine;

public class CreditRoller : MonoBehaviour
{
    public float scrollSpeed = 50f; // ความเร็วในการเลื่อน (พิกเซลต่อวินาที)
    public float returnTime = 5f;   // เวลาก่อนกลับไปหน้าเมนู (เป็นวินาที)

    private float timer;

    void Start()
    {
        timer = returnTime;
    }

    void Update()
    {
        // เลื่อนเครดิตขึ้น
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);

        // นับถอยหลังเพื่อกลับไปหน้าเมนู
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ReturnToMenu();
        }
    }

    void ReturnToMenu()
    {
        // เปลี่ยนชื่อ "MenuScene" เป็นชื่อ Scene เมนูหลักของคุณ
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }
}