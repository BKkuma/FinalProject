using UnityEngine;
using UnityEngine.EventSystems;

public class StartButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject startButton;

    private void Start()
    {
        startButton.SetActive(false); // ปิดตอนเริ่ม
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        startButton.SetActive(true); // โผล่ตอนเมาส์ตรง
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        startButton.SetActive(false); // ซ่อนเมื่อออก
    }
}
