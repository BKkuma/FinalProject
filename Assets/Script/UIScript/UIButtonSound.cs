using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // ต้องมีเพื่อเข้าถึง Button

// ⭐ Interfaces ที่จำเป็นสำหรับดักจับเหตุการณ์ของเมาส์
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    [Tooltip("ลากไฟล์เสียงสำหรับเมื่อเมาส์ชี้มาใส่ที่นี่")]
    public AudioClip hoverSound;
    [Tooltip("ลากไฟล์เสียงสำหรับเมื่อคลิกปุ่มมาใส่ที่นี่")]
    public AudioClip clickSound;

    private AudioSource audioSource;
    private Button button;

    void Awake()
    {
        // 1. ดึง AudioSource จาก GameObject นี้ (ถ้ามี) หรือเพิ่มใหม่
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            // ตั้งค่าให้เล่นเป็นเสียง UI/SFX (ไม่วนซ้ำ, ไม่ใช่ 3D)
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        // 2. ดึง Button component (เพื่อใช้ในการตรวจสอบว่าปุ่มถูกปิดใช้งานหรือไม่)
        button = GetComponent<Button>();
    }

    // ⭐ IPointerEnterHandler: ถูกเรียกเมื่อเมาส์ชี้ไปที่ปุ่ม
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ตรวจสอบว่าปุ่มถูกเปิดใช้งานอยู่หรือไม่ (interactable = true)
        if (button != null && !button.interactable)
            return;

        if (hoverSound != null)
        {
            // ใช้ PlayOneShot เพื่อให้เล่นเสียงทับกันได้ โดยไม่หยุดเสียงอื่นที่เล่นอยู่
            audioSource.PlayOneShot(hoverSound);
        }
    }

    // ⭐ IPointerClickHandler: ถูกเรียกเมื่อคลิกปุ่ม
    public void OnPointerClick(PointerEventData eventData)
    {
        // ตรวจสอบว่าปุ่มถูกเปิดใช้งานอยู่หรือไม่
        if (button != null && !button.interactable)
            return;

        if (clickSound != null)
        {
            // ใช้ PlayOneShot
            audioSource.PlayOneShot(clickSound);
        }
    }

    // ฟังก์ชันสำหรับลูกศร/เน้นด้วยการชี้: ไม่จำเป็นต้องใช้โค้ดเพิ่มเติม
    // เพราะ IPointerEnterHandler ก็ทำหน้าที่นี้ได้ หากผู้ใช้เลื่อนลูกศรจนมาถึงปุ่มนั้น
}