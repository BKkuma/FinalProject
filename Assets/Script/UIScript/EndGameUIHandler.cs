using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class EndGameUIHandler : MonoBehaviour
{
   
    private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [Tooltip("ระยะเวลาที่ใช้ในการ Fade In หรือ Fade Out")]
    public float fadeDuration = 1.0f;

    [Header("Next Scene/Action")]
    [Tooltip("ชื่อ Scene ที่ต้องการโหลดเมื่อกดปุ่ม (ถ้ามี)")]
    public string menuSceneName = "MainMenu";

   
    private void Awake()
    {
        
        if (canvasGroup == null)
        {
            Debug.LogError("FATAL ERROR: The CanvasGroup must be manually assigned in the Inspector for " + gameObject.name);
            return;
        }

        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    
    public IEnumerator FadeIn()
    {
        
        if (canvasGroup == null)
        {
            Debug.LogError(gameObject.name + ": CanvasGroup is NULL during FadeIn.");
            yield break; // หยุด Coroutine ทันที
        }

        
        gameObject.SetActive(true);

       
        canvasGroup.blocksRaycasts = true;

        
        yield return StartCoroutine(FadeCanvasGroup(1f));

        
        canvasGroup.interactable = true;
    }

    /// <summary>
    /// เริ่มการ Fade Out หน้าจอ UI นี้
    /// </summary>
    /// <param name="disableOnFinish">ปิด GameObject เมื่อ Fade Out เสร็จสิ้น</param>
    public IEnumerator FadeOut(bool disableOnFinish = true)
    {
        
        if (canvasGroup == null)
        {
            Debug.LogError(gameObject.name + ": CanvasGroup is NULL during FadeOut.");
            yield break; 
        }

       
        canvasGroup.interactable = false;

       
        yield return StartCoroutine(FadeCanvasGroup(0f));

       
        canvasGroup.blocksRaycasts = false;
        if (disableOnFinish)
        {
            gameObject.SetActive(false);
        }
    }

    
    private IEnumerator FadeCanvasGroup(float endAlpha)
    {
      
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

       
        canvasGroup.alpha = endAlpha;
    }

   
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