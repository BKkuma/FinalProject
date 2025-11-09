using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuBGSlideUI : MonoBehaviour
{
    public RectTransform bg1;
    public float targetY = 0f;
    public float speed = 1000f;
    public Button startButton;
    public Button exitButton;

    [Header("Sound Settings")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip doorCloseSFX;
    [SerializeField] private AudioSource bgmSource;

    private bool slideDone = false;
    private bool soundPlayed = false;

    void Start()
    {
        bg1.anchoredPosition = new Vector2(bg1.anchoredPosition.x, 1000f);

        startButton.interactable = false;
        exitButton.interactable = false;

        if (bgmSource != null)
            bgmSource.Stop();
    }

    void Update()
    {
        if (!slideDone)
        {
            Vector2 pos = bg1.anchoredPosition;
            pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
            bg1.anchoredPosition = pos;

            if (pos.y == targetY)
            {
                slideDone = true;

                if (!soundPlayed && sfxSource != null && doorCloseSFX != null)
                {
                    sfxSource.PlayOneShot(doorCloseSFX);
                    soundPlayed = true;
                    StartCoroutine(PlayBGMWithDelay(doorCloseSFX.length));
                }

                startButton.interactable = true;
                exitButton.interactable = true;
            }
        }
    }

    IEnumerator PlayBGMWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bgmSource != null)
            bgmSource.Play();
    }
}
