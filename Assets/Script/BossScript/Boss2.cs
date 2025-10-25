using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss2 : MonoBehaviour
{
    [Header("Movement & Warp Points")]
    public Transform[] warpPoints;       // จุดวาร์ปปกติ
    public Transform centerPoint;        // จุดกลางอากาศช่วง Frenzy
    public Transform overloadPoint;      // จุดลงหลังบ้าครั่ง
    public float warpDelay = 5f;         // เวลายืนเฉยหลังยิง
    public float warpSpeed = 10f;        // ความเร็ววาร์ป (ใช้เป็น Instant ตอนนี้)

    [Header("Attack")]
    public GameObject energyBallPrefab;
    public Transform[] energyBallSpawnPoints;
    public int phase1Balls = 3;
    public int phase2Balls = 5;
    public float energyBallDelay = 0.2f;

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

    void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        StartCoroutine(BossRoutine());
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

    // ---------------- Phase 1 ----------------
    IEnumerator Phase1Routine()
    {
        while (currentHP > maxHP / 2)
        {
            yield return StartCoroutine(WarpAndShoot(phase1Balls));
        }
        currentPhase = Phase.Frenzy;
    }

    // ---------------- Frenzy (บ้าครั่ง) ----------------
    IEnumerator FrenzyRoutine()
    {
        // วาร์ปไปกลางอากาศ
        yield return StartCoroutine(WarpTo(centerPoint.position));

        isInvincible = true;
        float timer = 0f;
        float frenzyDuration = 30f;

        while (timer < frenzyDuration)
        {
            // ยิง EnergyBall จาก spawn points แบบสุ่มต่อเนื่อง
            Transform spawnPoint = energyBallSpawnPoints[Random.Range(0, energyBallSpawnPoints.Length)];
            Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);
            timer += energyBallDelay;
            yield return new WaitForSeconds(energyBallDelay);
        }

        isInvincible = false;
        currentPhase = Phase.Overload;
    }

    // ---------------- Overload (ลงมาและยืนเฉย) ----------------
    IEnumerator OverloadRoutine()
    {
        // วาร์ปไปจุด Overload
        yield return StartCoroutine(WarpTo(overloadPoint.position));

        // ยืนเฉย 10 วิ
        yield return new WaitForSeconds(10f);

        currentPhase = Phase.Phase2;
    }

    // ---------------- Phase 2 ----------------
    IEnumerator Phase2Routine()
    {
        // Phase 2 ยากขึ้น: วาร์ปเร็วขึ้น, ยิงเร็วขึ้น
        float phase2WarpDelay = warpDelay / 2f;
        float phase2EnergyBallDelay = energyBallDelay / 2f;

        while (currentHP > 0)
        {
            // วาร์ป + ยิงลูกบอล
            Transform target = GetRandomWarpPoint();
            yield return StartCoroutine(WarpTo(target.position));

            // ยิง EnergyBall จำนวนมากขึ้น
            List<Transform> availablePoints = new List<Transform>(energyBallSpawnPoints);
            int count = Mathf.Min(phase2Balls, availablePoints.Count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, availablePoints.Count);
                Transform spawnPoint = availablePoints[index];
                Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);
                availablePoints.RemoveAt(index);
                yield return new WaitForSeconds(phase2EnergyBallDelay);
            }

            yield return new WaitForSeconds(phase2WarpDelay);
        }
    }

    // ---------------- Warp Helper ----------------
    IEnumerator WarpAndShoot(int balls)
    {
        Transform target = GetRandomWarpPoint();
        yield return StartCoroutine(WarpTo(target.position));

        // ยิง EnergyBall จาก spawn points แบบสุ่มไม่ซ้ำ
        List<Transform> availablePoints = new List<Transform>(energyBallSpawnPoints);
        int count = Mathf.Min(balls, availablePoints.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[index];
            Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);
            availablePoints.RemoveAt(index);
            yield return new WaitForSeconds(energyBallDelay);
        }

        yield return new WaitForSeconds(warpDelay);
    }

    IEnumerator WarpTo(Vector3 targetPos)
    {
        transform.position = targetPos;
        yield return null;
    }

    Transform GetRandomWarpPoint()
    {
        Transform[] options;
        if (warpPoints.Length > 1)
        {
            options = System.Array.FindAll(warpPoints, p => p != lastWarpPoint);
        }
        else
        {
            options = warpPoints;
        }

        Transform target = options[Random.Range(0, options.Length)];
        lastWarpPoint = target;
        return target;
    }

    // ---------------- Take Damage ----------------
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

    // ---------------- Collision กับกระสุน Player ----------------
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
