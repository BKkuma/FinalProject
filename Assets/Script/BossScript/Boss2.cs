using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
// ⭐ ต้องมีบรรทัดนี้เพื่อใช้งาน SceneManager
using UnityEngine.SceneManagement;

public class Boss2 : MonoBehaviour
{
    // Event สำหรับสคริปต์อื่น ๆ เพื่อใช้ดักจับตอนที่บอสตายแล้ว
    public event Action onBossDefeated;

    [Header("Phase 2 Intro Settings")]
    public bool waitForAnimation = true;
    public float autoStartDelay = 3f;
    public Animator bossAnimator;
    public string introAnimationName = "Transform";

    [Header("New Attack & Decoration")]
    [Tooltip("ชื่อ Animation ท่าโจมตีใหม่ที่เล่นวนลูปตลอดการต่อสู้")]
    public string superAttackAnimationName = "BossFightLoop";
    [Tooltip("Prefab ก้อนพลังงานกลมๆ ที่จะอยู่ด้านหลังตลอดการต่อสู้")]
    public GameObject decorationPrefab;
    [Tooltip("ค่า Offset แกน Y เพื่อปรับตำแหน่งของตกแต่งให้สูงขึ้นหรือต่ำลง")]
    public float decorationOffsetY = 0.5f; // ⭐ NEW

    [Header("Movement & Warp Points")]
    public Transform[] warpPoints;
    public Transform centerPoint;
    public Transform overloadPoint;
    public float moveSpeed = 20f;
    public float phase2SpeedMultiplier = 1.5f;
    public float warpDelay = 5f;

    [Header("Frenzy Special Attack")]
    [Tooltip("GameObject ของ Platform ที่ต้องการให้เลื่อนขึ้นมา")]
    public GameObject playerPlatform;
    [Tooltip("ตำแหน่งที่ Platform ควรจะไปถึงเมื่อเริ่ม Frenzy")]
    public Vector3 platformMoveToPosition;
    public float platformMoveDuration = 1.5f;
    [Tooltip("Prefab ท่าโจมตีแนวนอน (เช่น Laser หรือ Projectile ขนาดใหญ่)")]
    public GameObject frenzyHorizontalAttackPrefab;
    [Tooltip("ตำแหน่งยิงของท่าโจมตีแนวนอน (น่าจะอยู่ข้างบอส)")]
    public Transform horizontalAttackSpawnPoint;
    [Tooltip("เวลาชาร์จก่อนยิงท่าแนวนอน")]
    public float frenzyHorizontalAttackCharge = 3f;
    // ⭐ NEW: ระยะเวลาที่ Laser ค้างอยู่บนจอ
    [Tooltip("ระยะเวลาที่ Laser ค้างอยู่บนจอ")]
    public float frenzyHorizontalAttackDuration = 3f;
    [Tooltip("ความถี่ในการโจมตีแบบแนวนอน (ช่วงพักระหว่างการโจมตี)")]
    public float frenzyHorizontalAttackInterval = 3f;

    [Header("Phase 2 Enemy Spawner")]
    [Tooltip("Prefab ของศัตรูที่จะถูกสุ่มเสกใน Phase 2")]
    public GameObject phase2EnemyPrefab;
    [Tooltip("Transform ของจุดเกิดศัตรู (กำหนด 6 จุดใน Inspector)")]
    public Transform[] phase2SpawnPoints;
    [Tooltip("ความถี่ในการเสกศัตรู (หน่วยเป็นวินาที, คุณร้องขอ 5 วินาที)")]
    public float spawnInterval = 5f;
    private Coroutine enemySpawnRoutine;

    [Header("Attack")]
    public GameObject energyBallPrefab;
    public Transform[] energyBallSpawnPoints;
    public int phase1Balls = 3;
    public int phase2Balls = 5;
    public float energyBallDelay = 0.2f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip chargeSFX;      // เสียงชาร์จพลัง
    public AudioClip shootSFX_Phase1;    // เสียงยิงปกติ
    public AudioClip shootSFX_Frenzy;    // เสียงใหม่: เสียงยิงรัวตอนอยู่กลางจอ
    public float chargeDuration = 0.5f;

    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Hit Feedback")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // ⭐ MODIFIED: Defeat & Victory Settings (ตัด UI Handler ออกไป)
    [Header("Defeat & Victory")]
    public float deathAnimationDuration = 3f;
    public GameObject explosionPrefab;
    [Tooltip("ชื่อ Scene ฉากจบ (เช่น VictoryScene)")]
    public string victorySceneName = "VictoryScene"; // ⭐ NEW: สำหรับ LoadScene
    // ⭐ END MODIFIED

    private enum Phase { Phase1, Frenzy, Overload, Phase2 }
    private Phase currentPhase = Phase.Phase1;

    private Coroutine hitFlashRoutine;
    private bool isInvincible = false;
    private bool isDefeated = false; // ตัวแปรเช็คสถานะการตาย
    private Transform lastWarpPoint;

    private bool battleStarted = false;
    private GameObject permanentDecoration; // สำหรับเก็บ Prefab ลูกกลมๆ ที่ติดถาวร

    // สำหรับเก็บตำแหน่งเดิมของ Platform 
    private Vector3 platformOriginalPosition;

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

        // เก็บตำแหน่งเดิมของ Platform (ถ้ามี)
        if (playerPlatform != null)
        {
            platformOriginalPosition = playerPlatform.transform.position;
        }

        isInvincible = true;

        // เรียก StartBattleSequence เพื่อจัดการ Transform Animation และท่าโจมตีเริ่มต้น
        if (waitForAnimation)
        {
            if (bossAnimator != null) bossAnimator.Play(introAnimationName);
            Invoke("StartBattleSequence", autoStartDelay);
        }
        else
        {
            StartBattleSequence();
        }
    }

    // ฟังก์ชันที่ถูกเรียกเมื่อ Intro Animation จบ
    public void StartBattleSequence()
    {
        if (battleStarted) return;
        StartCoroutine(InitialBattleSequence());
    }

    // Coroutine ลำดับเหตุการณ์เริ่มต้น: เล่นท่าถาวร -> เริ่ม Phase 1
    IEnumerator InitialBattleSequence()
    {
        // 1. เล่นท่าโจมตีพิเศษและแสดงของตกแต่ง (ถาวร)
        yield return StartCoroutine(SuperAttackRoutine());
        if (isDefeated) yield break;

        // 2. เริ่มการต่อสู้หลัก
        battleStarted = true;
        isInvincible = false; // เปิดให้อ่อนแอลง
        currentPhase = Phase.Phase1; // เริ่ม Phase 1
        Debug.Log("Phase 1 Combat Started!");

        // 3. เข้าสู่ Coroutine วนลูปหลัก
        yield return StartCoroutine(BossRoutine());
    }


    // Coroutine สำหรับการติดตั้ง Animation และ Prefab ถาวร
    IEnumerator SuperAttackRoutine()
    {
        // ตรวจสอบว่าเคยสร้างของตกแต่งถาวรแล้วหรือยัง (เพื่อไม่ให้สร้างซ้ำ)
        if (permanentDecoration != null)
        {
            yield break;
        }

        Debug.Log("Starting Permanent Super Attack Animation and Decoration");

        // 1. เล่น Animation ท่าโจมตีใหม่ (Animation นี้ต้องตั้งค่าให้วนลูปใน Unity Animator)
        if (bossAnimator != null) bossAnimator.Play(superAttackAnimationName);

        // 2. สร้าง Prefab ของตกแต่งเป็น Child ของบอส
        if (decorationPrefab != null)
        {
            // Instantiates the decoration as a child of the boss's transform
            // เก็บ Reference ไว้ใน permanentDecoration
            permanentDecoration = Instantiate(decorationPrefab, transform.position, Quaternion.identity, transform);

            // 3. ปรับตำแหน่ง Z เพื่อให้อยู่ด้านหลัง และ ปรับตำแหน่ง Y ด้วย decorationOffsetY
            permanentDecoration.transform.localPosition = new Vector3(0, decorationOffsetY, 1f); // ⭐ MODIFIED
        }

        // Coroutine นี้จะจบลงทันที ทำให้การต่อสู้หลักสามารถดำเนินต่อไปได้ทันที
        yield break;
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

            // ตรวจสอบว่าบอสตายหรือไม่ เพื่อหยุด Coroutine หลัก
            if (isDefeated) yield break;
        }
    }

    IEnumerator Phase1Routine()
    {
        while (currentHP > maxHP / 2)
        {
            yield return StartCoroutine(MoveAndShoot(phase1Balls, moveSpeed));
            if (isDefeated) yield break;
        }
        currentPhase = Phase.Frenzy;
    }

    IEnumerator FrenzyRoutine()
    {
        Debug.Log("Frenzy Phase Started! Moving Platform and Boss. Using only Horizontal Attack.");

        // 1. เคลื่อนที่ไปกลางจอ
        yield return StartCoroutine(MoveTo(centerPoint.position, moveSpeed * 1.2f));
        if (isDefeated) yield break;

        // 2. NEW: เลื่อน Platform ขึ้นมา
        if (playerPlatform != null)
        {
            yield return StartCoroutine(MovePlatform(playerPlatform.transform, platformMoveToPosition, platformMoveDuration));
        }

        isInvincible = true;
        float timer = 0f;
        float frenzyDuration = 30f;

        // NEW: วนลูปยิงท่าโจมตีแนวนอนเท่านั้น
        while (timer < frenzyDuration)
        {
            // ⭐ [FIX 4] คำนวณเวลาที่ใช้ในการโจมตีหนึ่งรอบ (ชาร์จ + ยิงค้าง)
            float attackTime = frenzyHorizontalAttackCharge + frenzyHorizontalAttackDuration;

            yield return StartCoroutine(FrenzyHorizontalAttack());

            // เพิ่มเวลาที่ใช้ในการโจมตีหนึ่งรอบ
            timer += attackTime;

            // หน่วงเวลาระหว่างรอบการโจมตี
            yield return new WaitForSeconds(frenzyHorizontalAttackInterval);
            timer += frenzyHorizontalAttackInterval;

            if (isDefeated) yield break;
        }

        // 3. NEW: เลื่อน Platform กลับลงไป
        if (playerPlatform != null)
        {
            yield return StartCoroutine(MovePlatform(playerPlatform.transform, platformOriginalPosition, platformMoveDuration));
        }

        isInvincible = false;
        currentPhase = Phase.Overload;
    }

    // Coroutine สำหรับการโจมตีแนวนอน Frenzy (ชาร์จ 3 วินาที)
    IEnumerator FrenzyHorizontalAttack()
    {
        Debug.Log("Frenzy Horizontal Attack Charged! Waiting for " + frenzyHorizontalAttackCharge + " seconds.");

        // 1. ชาร์จ (3 วินาที)
        PlaySound(chargeSFX);
        yield return new WaitForSeconds(frenzyHorizontalAttackCharge);
        if (isDefeated) yield break;

        // 2. ยิงท่าโจมตีแนวนอน
        if (frenzyHorizontalAttackPrefab != null && horizontalAttackSpawnPoint != null)
        {
            // Instantiates ท่าโจมตีที่ตำแหน่งยิงที่กำหนด
            // ⭐ [FIX 1] เก็บ Reference ของ Laser ที่ถูกสร้างขึ้นมา ⭐
            GameObject laserInstance = Instantiate(frenzyHorizontalAttackPrefab, horizontalAttackSpawnPoint.position, Quaternion.identity);

            // เสียงยิง
            PlaySound(shootSFX_Frenzy);

            // ⭐ [FIX 2] รอให้ Laser ค้างอยู่บนจอตามเวลาที่กำหนด ⭐
            Debug.Log($"Laser Fired! Active for {frenzyHorizontalAttackDuration} seconds.");
            yield return new WaitForSeconds(frenzyHorizontalAttackDuration);

            // ⭐ [FIX 3] ทำลาย Laser หลังจากหมดเวลา ⭐
            if (laserInstance != null)
            {
                Destroy(laserInstance);
                Debug.Log("Laser destroyed.");
            }
        }
        else
        {
            // หากไม่มี Prefab/SpawnPoint ต้องรอเวลาค้างไว้ด้วย มิฉะนั้นจะยิงเร็วเกินไป
            yield return new WaitForSeconds(frenzyHorizontalAttackDuration);
        }

        // 🔴 ลบ: 'yield return new WaitForSeconds(1f);' ออก
    }

    IEnumerator OverloadRoutine()
    {
        yield return StartCoroutine(MoveTo(overloadPoint.position, moveSpeed));
        if (isDefeated) yield break;

        // 🔴 เริ่ม Coroutine การเสกมอนสเตอร์ทันทีที่เข้าสู่ Overload/Phase 2
        if (phase2EnemyPrefab != null && phase2SpawnPoints.Length > 0)
        {
            enemySpawnRoutine = StartCoroutine(SpawnEnemiesRoutine());
            Debug.Log("Enemy Spawner Routine Started.");
        }

        yield return new WaitForSeconds(10f);
        if (isDefeated) yield break;
        currentPhase = Phase.Phase2;
    }

    // ⭐ Coroutine สำหรับการสุ่มเสกมอนสเตอร์จากจุดที่กำหนด ⭐
    IEnumerator SpawnEnemiesRoutine()
    {
        if (phase2EnemyPrefab == null)
        {
            Debug.LogError("Phase 2 Enemy Prefab is not set! Cannot spawn enemies.");
            yield break;
        }
        if (phase2SpawnPoints.Length == 0)
        {
            Debug.LogError("Phase 2 Spawn Points are not assigned! Cannot spawn enemies.");
            yield break;
        }

        while (!isDefeated)
        {
            // 1. สุ่มเลือกจุดเกิดจาก Array
            int randomIndex = UnityEngine.Random.Range(0, phase2SpawnPoints.Length);
            Transform spawnPoint = phase2SpawnPoints[randomIndex];

            // 2. เสกศัตรูที่ตำแหน่งของจุดเกิดที่สุ่มมา
            Instantiate(phase2EnemyPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("Spawned enemy at point: " + randomIndex);

            // 3. รอตามช่วงเวลาที่กำหนด
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator Phase2Routine()
    {
        float phase2WarpDelay = warpDelay / 2f;
        float phase2EnergyBallDelay = energyBallDelay / 2f;
        float phase2Speed = moveSpeed * phase2SpeedMultiplier;

        // การเสกศัตรูถูกเริ่มไปแล้วใน OverloadRoutine() 

        while (currentHP > 0)
        {
            Transform target = GetRandomWarpPoint();
            yield return StartCoroutine(MoveTo(target.position, phase2Speed));
            if (isDefeated) yield break;

            List<Transform> availablePoints = new List<Transform>(energyBallSpawnPoints);
            int count = Mathf.Min(phase2Balls, availablePoints.Count);

            // เสียงชาร์จ
            PlaySound(chargeSFX);
            yield return new WaitForSeconds(chargeDuration);
            if (isDefeated) yield break;

            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, availablePoints.Count);
                Transform spawnPoint = availablePoints[index];
                Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);

                // กลับมาใช้เสียง Phase 1
                PlaySound(shootSFX_Phase1);

                availablePoints.RemoveAt(index);
                yield return new WaitForSeconds(phase2EnergyBallDelay);
                if (isDefeated) yield break;
            }

            yield return new WaitForSeconds(phase2WarpDelay);
            if (isDefeated) yield break;
        }
    }

    IEnumerator MoveAndShoot(int balls, float speed)
    {
        Transform target = GetRandomWarpPoint();
        yield return StartCoroutine(MoveTo(target.position, speed));
        if (isDefeated) yield break;

        List<Transform> availablePoints = new List<Transform>(energyBallSpawnPoints);
        int count = Mathf.Min(balls, availablePoints.Count);

        PlaySound(chargeSFX);
        yield return new WaitForSeconds(chargeDuration);
        if (isDefeated) yield break;

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[index];
            Instantiate(energyBallPrefab, spawnPoint.position, Quaternion.identity);

            // ใช้เสียง Phase 1 ปกติ
            PlaySound(shootSFX_Phase1);

            availablePoints.RemoveAt(index);
            yield return new WaitForSeconds(energyBallDelay);
            if (isDefeated) yield break;
        }

        yield return new WaitForSeconds(warpDelay);
        if (isDefeated) yield break;
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
            if (isDefeated) yield break;
        }
        transform.position = targetPos;
    }

    // Coroutine สำหรับการเคลื่อนที่ Platform
    IEnumerator MovePlatform(Transform platform, Vector3 targetPos, float duration)
    {
        if (platform == null) yield break;

        Vector3 startPos = platform.position;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            platform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        platform.position = targetPos;
    }

    Transform GetRandomWarpPoint()
    {
        Transform[] options;
        if (warpPoints.Length > 1)
            options = System.Array.FindAll(warpPoints, p => p != lastWarpPoint);
        else
            options = warpPoints;

        Transform target = options[UnityEngine.Random.Range(0, options.Length)];
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
        if (isInvincible || isDefeated) return;

        currentHP -= dmg;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);
        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            // เมื่อ HP หมด ให้เรียก Die()
            Die();
        }
    }

    void Die()
    {
        if (isDefeated) return;

        // 🔴 หยุด Coroutine การเสกมอนสเตอร์เมื่อบอสตาย
        if (enemySpawnRoutine != null)
        {
            StopCoroutine(enemySpawnRoutine);
            enemySpawnRoutine = null;
            Debug.Log("Enemy Spawner Routine Stopped.");
        }

        // หยุด Coroutine การต่อสู้ทั้งหมดที่เหลือ
        StopAllCoroutines();

        // 4. ทำลายของตกแต่งถาวร
        if (permanentDecoration != null)
        {
            Destroy(permanentDecoration);
        }

        // ป้องกันความเสียหายเพิ่มเติมและตั้งสถานะ
        isInvincible = true;
        isDefeated = true;

        // เริ่มฉากจบ
        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        Debug.Log("Boss2 defeated! Starting Victory Sequence (Loading Scene).");

        // 1. ระเบิด/เล่น Animation ตาย และรอฉากจบ/ระเบิดเสร็จ
        if (bossAnimator != null)
        {
            // bossAnimator.SetTrigger("Death"); // เรียก Animation ตาย
        }

        // โค้ดระเบิด
        if (explosionPrefab != null)
        {
            int numExplosions = 10;
            for (int i = 0; i < numExplosions; i++)
            {
                Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0);
                Instantiate(explosionPrefab, transform.position + randomOffset, Quaternion.identity);
                yield return new WaitForSeconds(deathAnimationDuration / numExplosions);
            }
        }

        // 2. แจ้งเตือนสคริปต์อื่น ๆ ว่าบอสตายแล้ว
        onBossDefeated?.Invoke();

        // 3. ทำลาย Object บอสหลัก
        Destroy(gameObject);

        // ⭐⭐⭐ 4. โหลดฉากจบใหม่ทันที ⭐⭐⭐
        if (!string.IsNullOrEmpty(victorySceneName))
        {
            Debug.Log("Loading Victory Scene: " + victorySceneName);
            SceneManager.LoadScene(victorySceneName);
        }
        else
        {
            Debug.LogError("Victory Scene Name is not set! Please check Boss2 Inspector.");
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
        if (isDefeated) return;

        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            // สมมติว่า Bullet มี damage public field/property
            if (collision.gameObject.TryGetComponent<Bullet>(out Bullet playerBullet))
            {
                TakeDamage(playerBullet.damage);
                Destroy(collision.gameObject);
            }
        }
    }
}