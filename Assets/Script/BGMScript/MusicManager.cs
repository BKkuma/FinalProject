using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // เพิ่ม namespace นี้

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip normalMusic;
    public AudioClip bossMusic;

    [Header("Settings")]
    public float fadeDuration = 1.0f;

    private AudioSource audioSource;
    private Coroutine currentFade;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        // ไม่ต้องเรียก PlayNormalMusic() ที่นี่แล้ว เพราะ OnSceneLoaded จะทำงานแทน
    }

    // เพิ่มส่วนนี้เพื่อดักจับการเปลี่ยน Scene
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ทุกครั้งที่โหลด Scene (รวมถึง Restart เกม) ให้กลับมาเล่นเพลงปกติ
        PlayNormalMusic();
    }

    public void PlayNormalMusic()
    {
        if (normalMusic == null) return;
        // ถ้าเพลงที่เล่นอยู่คือ normalMusic อยู่แล้ว ไม่ต้อง fade ใหม่
        if (audioSource.clip == normalMusic && audioSource.isPlaying) return;

        StartFadeToClip(normalMusic);
    }

    public void PlayBossMusic()
    {
        if (bossMusic == null) return;
        if (audioSource.clip == bossMusic && audioSource.isPlaying) return;

        StartFadeToClip(bossMusic);
    }

    private void StartFadeToClip(AudioClip newClip)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeToClip(newClip));
    }

    private IEnumerator FadeToClip(AudioClip newClip)
    {
        float startVolume = 1.0f; // กำหนด volume มาตรฐาน หรือใช้ audioSource.volume เดิมก็ได้

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}