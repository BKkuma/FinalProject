using UnityEngine;
using System.Collections;

public class JetBoss : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;       
    public Transform leftPoint;
    public Transform rightPoint;
    public float sineAmplitude = 1.5f;  
    public float sineFrequency = 3f;   

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletFireRate = 0.3f;
    public int bulletSpreadCount = 3;   
    public float bulletSpreadAngle = 15f; 
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip machineGunSound;

    [Header("Stats")]
    public int maxHP = 50;
    private int currentHP;

    [Header("Hit Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;

    private bool movingRight = true;
    private Coroutine attackRoutine;
    private Coroutine hitFlashRoutine;
    private Color originalColor;

    public System.Action onPhase1Defeated;

    void Start()
    {
        currentHP = maxHP;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        attackRoutine = StartCoroutine(MachineGunAttack());
    }

    void Update()
    {
        MovePhase1();
    }

    void MovePhase1()
    {
        if (leftPoint == null || rightPoint == null) return;

        
        float step = moveSpeed * Time.deltaTime;
        Vector3 target = movingRight ? rightPoint.position : leftPoint.position;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        if (Vector3.Distance(transform.position, target) < 0.1f)
            movingRight = !movingRight;

       
        float sineY = Mathf.Sin(Time.time * sineFrequency) * sineAmplitude;
        transform.position = new Vector3(transform.position.x, transform.position.y + sineY * Time.deltaTime, transform.position.z);
    }

    IEnumerator MachineGunAttack()
    {
        while (currentHP > 0)
        {
            ShootBulletSpread();
            PlaySound(machineGunSound, 0.7f);
            yield return new WaitForSeconds(bulletFireRate);
        }
    }

    void ShootBulletSpread()
    {
        if (bulletPrefab == null || firePoint == null) return;

        for (int i = 0; i < bulletSpreadCount; i++)
        {
            float angle = -bulletSpreadAngle / 2f + (bulletSpreadAngle / (bulletSpreadCount - 1)) * i;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.left;
                rb.velocity = dir * 10f;
            }
        }
    }

    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
           
            onPhase1Defeated?.Invoke();
            
            StartCoroutine(EscapePhase1());
        }
    }

    IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
            hitFlashRoutine = null;
        }
    }

    IEnumerator EscapePhase1()
    {
        Vector3 escapePos = transform.position + new Vector3(0, -10f, 0);
        float t = 0f;
        Vector3 startPos = transform.position;
        float duration = 2f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, escapePos, t);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, t);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            Bullet playerBullet = collision.gameObject.GetComponent<Bullet>();
            if (playerBullet != null)
            {
                TakeDamage(playerBullet.damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
