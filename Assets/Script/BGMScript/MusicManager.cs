using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip normalMusic;
    // ⭐ NEW: แยกเพลงบอสตามหมายเลข ⭐
    public AudioClip bossMusic1;
    public AudioClip bossMusic2;
    public AudioClip bossMusic3;

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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayNormalMusic();
    }

    public void PlayNormalMusic()
    {
        if (normalMusic == null) return;
        if (audioSource.clip == normalMusic && audioSource.isPlaying) return;

        StartFadeToClip(normalMusic);
    }

    // ⭐ NEW: ฟังก์ชันสำหรับเล่นเพลงบอสโดยระบุหมายเลข ⭐
    public void PlayBossMusic(int bossNumber)
    {
        AudioClip selectedBossMusic = null;

        switch (bossNumber)
        {
            case 1:
                selectedBossMusic = bossMusic1;
                break;
            case 2:
                selectedBossMusic = bossMusic2;
                break;
            case 3:
                selectedBossMusic = bossMusic3;
                break;
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

    public void StopMusic()
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        audioSource.Stop();
        audioSource.volume = 1f;
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