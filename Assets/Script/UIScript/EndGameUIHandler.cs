using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class EndGameUIHandler : MonoBehaviour
{
    // ⭐ ตัวแปรสำคัญที่เราต้องอ้างอิงถึง CanvasGroup ของตัวเอง
    private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade In หรือ Fade Out")]
    public float fadeDuration = 1.0f;

    [Header("Next Scene/Action")]
    [Tooltip("ชื่อ Scene ที่ต้องการโหลดเมื่อกดปุ่ม (ถ้ามี)")]
    public string menuSceneName = "MainMenu";

    // ใช้ Awake() เพื่อดึง CanvasGroup มาทันที แม้ว่า GameObject จะปิดอยู่
    private void Awake()
    {
        // โค้ดนี้จะตรวจสอบว่าคุณลาก CanvasGroup มาใส่แล้วหรือไม่
        if (canvasGroup == null)
        {
            Debug.LogError("FATAL ERROR: The CanvasGroup must be manually assigned in the Inspector for " + gameObject.name);
            return;
        }

        // ตั้งค่าเริ่มต้น: ซ่อนไว้
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// เริ่มการ Fade In หน้าจอ UI นี้
    /// </summary>
    public IEnumerator FadeIn()
    {
        // ⭐ NEW: ป้องกัน NullReferenceException
        if (canvasGroup == null)
        {
            Debug.LogError(gameObject.name + ": CanvasGroup is NULL during FadeIn.");
            yield break; // หยุด Coroutine ทันที
        }

        // 1. เปิด GameObject
        gameObject.SetActive(true);

        // 2. เปิดให้บล็อก Raycasts
        canvasGroup.blocksRaycasts = true;

        // 3. เริ่ม Fade
        yield return StartCoroutine(FadeCanvasGroup(1f));

        // 4. เปิด Interactable หลัง Fade In เสร็จ (สำหรับ Final UI)
        canvasGroup.interactable = true;
    }

    /// <summary>
    /// เริ่มการ Fade Out หน้าจอ UI นี้
    /// </summary>
    /// <param name="disableOnFinish">ปิด GameObject เมื่อ Fade Out เสร็จสิ้น</param>
    public IEnumerator FadeOut(bool disableOnFinish = true)
    {
        // ⭐ NEW: ป้องกัน NullReferenceException
        if (canvasGroup == null)
        {
            Debug.LogError(gameObject.name + ": CanvasGroup is NULL during FadeOut.");
            yield break; // หยุด Coroutine ทันที
        }

        // 1. ปิดการโต้ตอบทันทีที่เริ่ม Fade Out
        canvasGroup.interactable = false;

        // 2. เริ่ม Fade
        yield return StartCoroutine(FadeCanvasGroup(0f));

        // 3. ปิดการบล็อก Raycasts และอาจจะปิด GameObject
        canvasGroup.blocksRaycasts = false;
        if (disableOnFinish)
        {
            gameObject.SetActive(false);
        }
    }

    // Coroutine หลักสำหรับการ Fade
    private IEnumerator FadeCanvasGroup(float endAlpha)
    {
        // ⭐ NEW: ป้องกัน NullReferenceException
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float startTime = Time.time;
        float endTime = startTime + fadeDuration;

        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        // กำหนดค่าสุดท้ายให้แน่ใจว่าตรงกับ endAlpha
        canvasGroup.alpha = endAlpha;
    }

    // *** ฟังก์ชันตัวอย่างที่ผูกกับปุ่ม UI ***
    public void OnRestartButtonClicked()
    {
        Debug.Log("Restarting current scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuButtonClicked()
    {
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            Debug.Log("Loading Main Menu: " + menuSceneName);
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("Menu Scene Name is not set!");
        }
    }
}