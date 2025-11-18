using UnityEngine;
using UnityEngine.UI;

public class MenuBGSlideUI : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform bg1;
    public Button startButton;
    public Button exitButton;

    [Header("Sound Settings")]
    [SerializeField] private AudioSource bgmSource;

    void Start()
    {
        // BG อยู่ตำแหน่งปกติ ไม่ต้องขยับอะไร
        // bg1 จะไม่ถูกเลื่อนอีกแล้ว

        // ปุ่มกดได้ทันที
        startButton.interactable = true;
        exitButton.interactable = true;

        // เล่นเพลงทันที ถ้ามี
        if (bgmSource != null)
        {
            bgmSource.Play();
        }
    }
}
