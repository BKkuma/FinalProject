using UnityEngine;
using System.Collections;

public class UIFadeIn : MonoBehaviour
{
    [Header("Settings")]
    public CanvasGroup targetCanvasGroup;
    public float fadeDuration = 2.0f;     
    public float delayStart = 0.5f;       

    void Start()
    {
        
        if (targetCanvasGroup == null)
            targetCanvasGroup = GetComponent<CanvasGroup>();

        
        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = 0f;
           
            StartCoroutine(FadeSequence());
        }
    }

    IEnumerator FadeSequence()
    {
        
        yield return new WaitForSeconds(delayStart);

        float timer = 0f;

       
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            
            float progress = timer / fadeDuration;

           
            targetCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

       
        targetCanvasGroup.alpha = 1f;
    }
}