using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip normalMusic;
    public AudioClip bossMusic;

    [Header("Settings")]
    public float fadeDuration = 1.0f; // ระยะเวลา fade in/out

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
        PlayNormalMusic();
    }

    public void PlayNormalMusic()
    {
        if (normalMusic == null) return;
        StartFadeToClip(normalMusic);
    }

    public void PlayBossMusic()
    {
        if (bossMusic == null) return;
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
        float startVolume = audioSource.volume;

        // 🔉 Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.clip = newClip;
        audioSource.Play();

        // 🔊 Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }
}
