using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip normalMusic;
    public AudioClip bossMusic1;
    public AudioClip bossMusic2;
    public AudioClip bossMusic3;

    [Header("Scene Control")] // ⭐ NEW: เปลี่ยนชื่อ Header
    [Tooltip("ชื่อ Scene ที่ MusicManager ควรจะทำงานและเล่นเพลงปกติ (เช่น Level1)")]
    public string primaryGameSceneName = "Level1"; // ⭐ NEW: ระบุชื่อ Scene หลักที่ใช้ควบคุมเพลง
    [Tooltip("ชื่อ Scene ที่ไม่ควรให้ MusicManager เล่นเพลงปกติ (เช่น VictoryScene)")]
    public string victorySceneName = "VictoryScene";

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
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ⭐ MODIFIED: ตรรกะใหม่: ทำงานเฉพาะใน PrimaryGameSceneName เท่านั้น ⭐
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. ตรวจสอบ Scene ที่โหลด
        if (scene.name == primaryGameSceneName)
        {
            // ถ้าเข้า Scene หลัก: เล่นเพลงปกติ
            PlayNormalMusic();
        }
        else
        {
            // ถ้าเข้า Scene อื่นที่ไม่ใช่ Scene หลัก (รวมถึง Scene ชนะ): หยุดเพลงทันที
            // นี่คือการ "แบน" จาก Scene อื่นๆ
            StopMusic(instant: true);
            Debug.Log($"MusicManager: Entered Scene '{scene.name}'. Not the primary scene ({primaryGameSceneName}), music stopped.");
        }
    }

    public void PlayNormalMusic()
    {
        // ⭐ NEW: ตรวจสอบก่อนเล่นอีกครั้งว่าอยู่ใน Scene หลักหรือไม่
        if (SceneManager.GetActiveScene().name != primaryGameSceneName)
        {
            return;
        }

        if (normalMusic == null) return;
        if (audioSource.clip == normalMusic && audioSource.isPlaying) return;

        StartFadeToClip(normalMusic);
    }

    // ฟังก์ชัน PlayBossMusic (คงเดิม)
    public void PlayBossMusic(int bossNumber)
    {
        // ตรวจสอบ Scene หลักก่อนเริ่มเพลงบอส
        if (SceneManager.GetActiveScene().name != primaryGameSceneName)
        {
            return;
        }

        AudioClip selectedBossMusic = null;

        switch (bossNumber)
        {
            case 1: selectedBossMusic = bossMusic1; break;
            case 2: selectedBossMusic = bossMusic2; break;
            case 3: selectedBossMusic = bossMusic3; break;
            default:
                Debug.LogWarning("Invalid boss number provided. Playing normal music instead.");
                PlayNormalMusic();
                return;
        }

        if (selectedBossMusic == null)
        {
            Debug.LogError($"Boss Music {bossNumber} clip is missing! Cannot play.");
            return;
        }

        if (audioSource.clip == selectedBossMusic && audioSource.isPlaying) return;

        StartFadeToClip(selectedBossMusic);
    }

    public void StopMusic(bool instant = false)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        if (instant)
        {
            audioSource.Stop();
            audioSource.volume = 1f;
            return;
        }

        currentFade = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float originalVolume = 1f;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = originalVolume;
    }

    private void StartFadeToClip(AudioClip newClip)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeToClip(newClip));
    }

    private IEnumerator FadeToClip(AudioClip newClip)
    {
        float startVolume = 1.0f;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}