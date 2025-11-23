using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class CreditSceneManager : MonoBehaviour
{
    [Header("Door UI Settings")]
    [Tooltip("ลาก RectTransform ของ DoorSlide UI (DoorSlide2) มาใส่ที่นี่")]
    public RectTransform doorPanel;

    // ตำแหน่งประตู
    public float offScreenUpYPosition = 1080f;
    public float closedYPosition = 0f;
    public float slideDuration = 0.5f;

    [Header("Timing")]
    public float closedHoldDuration = 2.0f;

    [Header("Door Audio")]
    [Tooltip("ลาก Audio Clip เสียงประตูเลื่อน (Sliding)")]
    public AudioClip slideSound;
    [Tooltip("ลาก Audio Clip เสียงปิดกระแทก (Slam/Impact)")]
    public AudioClip slamSound;

    [Header("Background Music")]
    [Tooltip("ลาก Audio Clip เพลงพื้นหลัง (BGM) ของฉากเครดิต")]
    public AudioClip bgmClip; // ⭐ NEW: สำหรับเพลง BGM ของฉากเครดิต

    [Header("Scene Loading")]
    public string menuSceneName = "Scenes/MenuScene";

    private AudioSource audioSource;
    private AudioSource bgmAudioSource; // ⭐ NEW: ใช้ AudioSource แยกสำหรับ BGM

    void Awake()
    {
        // ⭐ 1. เตรียม Audio Source สำหรับ SFX (เสียงประตู) ⭐
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        // ⭐ 2. เตรียม Audio Source สำหรับ BGM ⭐
        // แนะนำให้ใช้ AudioSource แยกสำหรับ BGM เพื่อให้เล่นพร้อมกับ SFX ได้
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true; // BGM ควรเล่นวนซ้ำ

        // 3. ตั้งค่าตำแหน่งเริ่มต้น
        if (doorPanel != null)
        {
            doorPanel.localPosition = new Vector3(doorPanel.localPosition.x, offScreenUpYPosition, doorPanel.localPosition.z);
        }
    }

    void Start()
    {
        // ⭐ ไม่ต้องเรียก Start BGM ที่นี่ เพราะจะเรียกใน Coroutine
        StartCoroutine(DoorIntroSequence());
    }

    IEnumerator DoorIntroSequence()
    {
        // 1. **เลื่อนประตูลง (ปิดฉาก) พร้อมเสียง**
        if (slideSound != null)
        {
            audioSource.clip = slideSound;
            audioSource.Play();
        }
        yield return StartCoroutine(SlideDoor(closedYPosition, slideDuration));

        // หยุดเสียงเลื่อน และเล่นเสียงปิด/กระแทก
        if (audioSource.isPlaying) audioSource.Stop();
        if (slamSound != null) audioSource.PlayOneShot(slamSound);

        // 2. **ค้างอยู่ (Hold)**
        yield return new WaitForSeconds(closedHoldDuration);

        // 3. **เลื่อนประตูขึ้น (เปิดเต็มที่เพื่อ Reveal) พร้อมเสียง**
        if (slideSound != null)
        {
            audioSource.clip = slideSound;
            audioSource.Play();
        }
        yield return StartCoroutine(SlideDoor(offScreenUpYPosition, slideDuration));

        // 4. **จบการเปิดเผย**
        if (audioSource.isPlaying) audioSource.Stop();
        if (doorPanel != null) doorPanel.gameObject.SetActive(false);

        // ⭐ NEW: 5. เริ่มเล่น BGM ทันทีที่ประตูเปิดเสร็จ ⭐
        Debug.Log("Door sequence finished. Starting BGM.");
        if (bgmClip != null)
        {
            bgmAudioSource.clip = bgmClip;
            bgmAudioSource.Play();
        }

        // 6. **ฉากเครดิตค้างอยู่**
    }

    IEnumerator SlideDoor(float targetY, float duration)
    {
        if (doorPanel == null) yield break;

        Vector3 startPos = doorPanel.localPosition;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            doorPanel.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        doorPanel.localPosition = targetPos;
    }

    // ฟังก์ชันสำหรับปุ่มกลับเมนู
    public void LoadMenu()
    {
        // ⭐ Optional: สั่งให้ BGM หยุดก่อนโหลดฉากใหม่
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("Menu Scene Name is not set!");
        }
    }
}