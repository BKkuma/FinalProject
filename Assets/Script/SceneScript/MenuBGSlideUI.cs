using UnityEngine;
using UnityEngine.UI; // สำหรับปุ่ม

public class MenuBGSlideUI : MonoBehaviour
{
    public RectTransform bg1;      // BG ที่จะเลื่อน
    public float targetY = 0f;     // ตำแหน่งสุดท้าย
    public float speed = 1000f;    // ความเร็ว
    public Button startButton;     // ปุ่ม Start
    public Button exitButton;      // ปุ่ม Exit

    private bool slideDone = false;

    void Start()
    {
        // เริ่มจาก Y สูงกว่าหน้าจอ
        // ปรับตามตำแหน่งของ BG1 ของคุณ
        bg1.anchoredPosition = new Vector2(bg1.anchoredPosition.x, 1000f);

        // ปิดปุ่มชั่วคราว
        startButton.interactable = false;
        exitButton.interactable = false;
    }

    void Update()
    {
        if (!slideDone)
        {
            Vector2 pos = bg1.anchoredPosition;
            pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
            bg1.anchoredPosition = pos;

            if (pos.y == targetY)
            {
                slideDone = true;

                // เปิดปุ่มให้คลิกได้
                startButton.interactable = true;
                exitButton.interactable = true;
            }
        }
    }
}
