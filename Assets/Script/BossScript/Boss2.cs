using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss2 : MonoBehaviour
{
    [Header("Movement & Warp Points")]
    public Transform[] warpPoints;
    public Transform centerPoint;
    public Transform overloadPoint;
    public float moveSpeed = 20f;       // ความเร็วในการเคลื่อนที่ปกติ
    public float phase2SpeedMultiplier = 1.5f; // ความเร็วคูณตอน Phase 2
    public float warpDelay = 5f;

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
            yield return StartCoroutine(MoveAndShoot(phase1Balls, moveSpeed));
        }
        currentPhase = Phase.Frenzy;
    }

    // ---------------- Frenzy ----------------
    IEnumerator FrenzyRoutine()
    {
        yield return StartCoroutine(MoveTo(centerPoint.position, moveSpeed * 1.2f));

        isInvincible = true;
        float timer = 0f;
        float frenzyDuration = 30f;

        while (timer < frenzyDuration)
        {
            Transform spawnPoint = energyBallSpawnPoints[Random.Range(0, energyBallSpawnPoints.Length)];
            Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);
            timer += energyBallDelay;
            yield return new WaitForSeconds(energyBallDelay);
        }

        isInvincible = false;
        currentPhase = Phase.Overload;
    }

    // ---------------- Overload ----------------
    IEnumerator OverloadRoutine()
    {
        yield return StartCoroutine(MoveTo(overloadPoint.position, moveSpeed));
        yield return new WaitForSeconds(10f);
        currentPhase = Phase.Phase2;
    }

    // ---------------- Phase 2 ----------------
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

    // ---------------- Movement Helper ----------------
    IEnumerator MoveAndShoot(int balls, float speed)
    {
        Transform target = GetRandomWarpPoint();
        yield return StartCoroutine(MoveTo(target.position, speed));

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

    // ---------------- Damage ----------------
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
