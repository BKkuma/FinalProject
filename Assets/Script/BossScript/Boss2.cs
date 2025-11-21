using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss2 : MonoBehaviour
{
    [Header("Phase 2 Intro Settings")]
    public bool waitForAnimation = true;
    public float autoStartDelay = 3f;
    public Animator bossAnimator;
    public string introAnimationName = "Transform";

    [Header("Movement & Warp Points")]
    public Transform[] warpPoints;
    public Transform centerPoint;
    public Transform overloadPoint;
    public float moveSpeed = 20f;
    public float phase2SpeedMultiplier = 1.5f;
    public float warpDelay = 5f;

    [Header("Attack")]
    public GameObject energyBallPrefab;
    public Transform[] energyBallSpawnPoints;
    public int phase1Balls = 3;
    public int phase2Balls = 5;
    public float energyBallDelay = 0.2f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip chargeSFX;         // เสียงชาร์จพลัง
    public AudioClip shootSFX_Phase1;   // เสียงยิงปกติ (ใช้ทั้ง Phase 1 และ Phase 2)
    public AudioClip shootSFX_Frenzy;   // ** เสียงใหม่: เสียงยิงรัวตอนอยู่กลางจอ (ท่าไม้ตาย) **
    public float chargeDuration = 0.5f;

    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Hit Feedback")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private enum Phase { Phase1, Frenzy, Overload, Phase2 }
    private Phase currentPhase = Phase.Phase1;

    private Coroutine hitFlashRoutine;
    private bool isInvincible = false;
    private Transform lastWarpPoint;

    private bool battleStarted = false;

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        isInvincible = true;

        if (waitForAnimation)
        {
            if (bossAnimator != null) bossAnimator.Play(introAnimationName);
            Invoke("BeginPhase2Combat", autoStartDelay);
        }
        else
        {
            BeginPhase2Combat();
        }
    }

    public void BeginPhase2Combat()
    {
        if (battleStarted) return;
        battleStarted = true;
        isInvincible = false;
        StartCoroutine(BossRoutine());
        Debug.Log("Phase 2 Combat Started!");
    }

    IEnumerator BossRoutine()
    {
        while (true)
        {
            switch (currentPhase)
            {
                case Phase.Phase1:
                    yield return StartCoroutine(Phase1Routine());
                    break;
                case Phase.Frenzy:
                    yield return StartCoroutine(FrenzyRoutine());
                    break;
                case Phase.Overload:
                    yield return StartCoroutine(OverloadRoutine());
                    break;
                case Phase.Phase2:
                    yield return StartCoroutine(Phase2Routine());
                    break;
            }
            yield return null;
        }
    }

    IEnumerator Phase1Routine()
    {
        while (currentHP > maxHP / 2)
        {
            yield return StartCoroutine(MoveAndShoot(phase1Balls, moveSpeed));
        }
        currentPhase = Phase.Frenzy;
    }

    IEnumerator FrenzyRoutine()
    {
        // เคลื่อนที่ไปกลางจอ
        yield return StartCoroutine(MoveTo(centerPoint.position, moveSpeed * 1.2f));

        isInvincible = true;
        float timer = 0f;
        float frenzyDuration = 30f;

        // เล่นเสียงชาร์จ
        PlaySound(chargeSFX);
        yield return new WaitForSeconds(chargeDuration);

        while (timer < frenzyDuration)
        {
            Transform spawnPoint = energyBallSpawnPoints[Random.Range(0, energyBallSpawnPoints.Length)];
            Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);

            // 🎯 ใช้เสียง Frenzy (เสียงใหม่) สำหรับท่ายิงรัวนี้
            PlaySound(shootSFX_Frenzy);

            timer += energyBallDelay;
            yield return new WaitForSeconds(energyBallDelay);
        }

        isInvincible = false;
        currentPhase = Phase.Overload;
    }

    IEnumerator OverloadRoutine()
    {
        yield return StartCoroutine(MoveTo(overloadPoint.position, moveSpeed));
        yield return new WaitForSeconds(10f);
        currentPhase = Phase.Phase2;
    }

    IEnumerator Phase2Routine()
    {
        float phase2WarpDelay = warpDelay / 2f;
        float phase2EnergyBallDelay = energyBallDelay / 2f;
        float phase2Speed = moveSpeed * phase2SpeedMultiplier;

        while (currentHP > 0)
        {
            Transform target = GetRandomWarpPoint();
            yield return StartCoroutine(MoveTo(target.position, phase2Speed));

            List<Transform> availablePoints = new List<Transform>(energyBallSpawnPoints);
            int count = Mathf.Min(phase2Balls, availablePoints.Count);

            // เสียงชาร์จ
            PlaySound(chargeSFX);
            yield return new WaitForSeconds(chargeDuration);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, availablePoints.Count);
                Transform spawnPoint = availablePoints[index];
                Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);

                // 🎯 กลับมาใช้เสียง Phase 1 ตามที่ต้องการ
                PlaySound(shootSFX_Phase1);

                availablePoints.RemoveAt(index);
                yield return new WaitForSeconds(phase2EnergyBallDelay);
            }

            yield return new WaitForSeconds(phase2WarpDelay);
        }
    }

    IEnumerator MoveAndShoot(int balls, float speed)
    {
        Transform target = GetRandomWarpPoint();
        yield return StartCoroutine(MoveTo(target.position, speed));

        List<Transform> availablePoints = new List<Transform>(energyBallSpawnPoints);
        int count = Mathf.Min(balls, availablePoints.Count);

        PlaySound(chargeSFX);
        yield return new WaitForSeconds(chargeDuration);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[index];
            Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);

            // 🎯 ใช้เสียง Phase 1 ปกติ
            PlaySound(shootSFX_Phase1);

            availablePoints.RemoveAt(index);
            yield return new WaitForSeconds(energyBallDelay);
        }

        yield return new WaitForSeconds(warpDelay);
    }

    IEnumerator MoveTo(Vector3 targetPos, float speed)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * (speed / distance);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
    }

    Transform GetRandomWarpPoint()
    {
        Transform[] options;
        if (warpPoints.Length > 1)
            options = System.Array.FindAll(warpPoints, p => p != lastWarpPoint);
        else
            options = warpPoints;

        Transform target = options[Random.Range(0, options.Length)];
        lastWarpPoint = target;
        return target;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isInvincible) return;

        currentHP -= dmg;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            Destroy(gameObject);
            Debug.Log("Boss2 defeated!");
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            spriteRenderer.color = originalColor;
        }
        hitFlashRoutine = null;
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