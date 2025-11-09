using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    public Transform targetPosition;      // จุดที่ player จะย้ายไป
    public float transitionDuration = 1.5f; // เวลาจอดำทั้งหมด
    public Image fadePanel;               // Panel สีดำ (ต้องตั้งใน Canvas)
    public AudioClip transitionSound;     // เสียงตอนย้ายด่าน
    [Range(0f, 1f)]
    public float transitionVolume = 1f;

    private bool isTransitioning = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTransitioning && other.CompareTag("Player"))
        {
            StartCoroutine(Transition(other.gameObject));
        }
    }

    IEnumerator Transition(GameObject player)
    {
        isTransitioning = true;

        // เล่นเสียงย้ายด่าน
        if (transitionSound != null)
            AudioSource.PlayClipAtPoint(transitionSound, Camera.main.transform.position, transitionVolume);

        // แสดง Panel จอดำ
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 0);
            float timer = 0f;
            while (timer < transitionDuration / 2f)
            {
                timer += Time.deltaTime;
                fadePanel.color = new Color(0, 0, 0, Mathf.Clamp01(timer / (transitionDuration / 2f)));
                yield return null;
            }
            fadePanel.color = Color.black;
        }

        // ปิด Player Movement ชั่วคราว
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        // ย้ายตำแหน่ง Player
        if (targetPosition != null)
        {
            player.transform.position = targetPosition.position;
        }

        // รออีกครึ่งเวลาเพื่อ fade out
        if (fadePanel != null)
        {
            float timer = 0f;
            while (timer < transitionDuration / 2f)
            {
                timer += Time.deltaTime;
                fadePanel.color = new Color(0, 0, 0, 1 - Mathf.Clamp01(timer / (transitionDuration / 2f)));
                yield return null;
            }
            fadePanel.color = new Color(0, 0, 0, 0);
            fadePanel.gameObject.SetActive(false);
        }

        // เปิด Player Movement กลับมา
        if (pm != null) pm.enabled = true;

        isTransitioning = false;
    }
}
