using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Source")]
    public AudioSource sfxSource;   // ตัวเล่นเสียง

    [Header("Sound Clips")]
    public AudioClip hoverClip;     // เสียงตอนชี้เมาส์
    public AudioClip clickClip;     // เสียงตอนกดปุ่ม

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sfxSource != null && hoverClip != null)
            sfxSource.PlayOneShot(hoverClip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sfxSource != null && clickClip != null)
            sfxSource.PlayOneShot(clickClip);
    }
}
