using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Fire Settings")]
    public Transform firePointRight, firePointLeft, firePointUp, firePointDown;
    public Transform crouchFirePoint;
    public Transform crouchFirePointRight, crouchFirePointLeft, crouchFirePointUp, crouchFirePointDown;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlashPrefab;
    private GameObject currentMuzzleFlash;

    [Header("Bullet Prefabs")]
    public GameObject normalBulletPrefab;
    public GameObject machineGunBulletPrefab;
    public GameObject shotgunBulletPrefab;
    public GameObject homingBulletPrefab;

    [Header("Fire Rates")]
    public float normalFireRate = 0.25f;
    public float machineGunFireRate = 0.1f;
    public float shotgunFireRate = 0.5f;
    private float nextFireTime = 0f;

    private PlayerMovement playerMove;
    private Animator animator;

    [Header("Ammo Settings")]
    public int machineGunAmmo = 0;
    public int shotgunAmmo = 0;
    public int homingAmmo = 0;

    private bool usingMachineGun = false;
    private bool usingShotgun = false;
    private bool usingHoming = false;
    private GameObject currentBulletPrefab;
    private bool isMachineGunFiring = false;

    [Header("Gun Sounds")]
    public AudioClip normalGunSound;
    public AudioClip machineGunSound;
    public AudioClip shotgunSound;
    public AudioClip homingGunSound;
    public AudioClip ammoEmptySound;
    private AudioSource audioSource;

    void Start()
    {
        playerMove = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        currentBulletPrefab = normalBulletPrefab;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update() => HandleShoot();

    void HandleShoot()
    {
        // ตัวแปรสำหรับเก็บว่าปืนพิเศษถูกกดแต่กระสุนหมดหรือไม่
        bool isSpecialGunEmpty = false;

        if (Input.GetKey(KeyCode.K))
        {
            if (animator != null) animator.SetBool("isShooting", true);

            // ------------------ Machine Gun (เสียงต่อเนื่อง) ------------------
            if (usingMachineGun)
            {
                if (machineGunAmmo > 0)
                {
                    if (!isMachineGunFiring)
                    {
                        audioSource.clip = machineGunSound;
                        audioSource.loop = true;
                        audioSource.Play();
                        isMachineGunFiring = true;
                    }
                }
                else // Machine Gun กระสุนหมด
                {
                    if (isMachineGunFiring)
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                        isMachineGunFiring = false;
                    }
                    isSpecialGunEmpty = true; // ตั้งค่าว่าปืนพิเศษกระสุนหมด
                }
            }

            // ------------------ Logic การยิงจริง ------------------
            if (Time.time >= nextFireTime)
            {
                Transform shootPoint = GetShootPoint();
                Vector2 direction = playerMove.ShootDirection;

                // ตรวจสอบกระสุนหมดของ Shotgun และ Homing ก่อน
                if (usingShotgun && shotgunAmmo <= 0)
                {
                    isSpecialGunEmpty = true;
                }
                else if (usingHoming && homingAmmo <= 0)
                {
                    isSpecialGunEmpty = true;
                }

                // ถ้าปืนพิเศษกระสุนหมด และไม่ใช่ Machine Gun ที่เพิ่งจัดการเสียงไปแล้ว
                if (isSpecialGunEmpty)
                {
                    // เล่นเสียงกระสุนหมด
                    if (ammoEmptySound != null && audioSource.clip != ammoEmptySound) // ตรวจสอบไม่ให้เล่นซ้ำถ้ายังคงกดค้าง
                    {
                        PlayGunSound(ammoEmptySound);
                        nextFireTime = Time.time + normalFireRate; // ใส่ delay เพื่อไม่ให้ spam เสียง
                    }
                }
                // ------------------ Logic ยิงปืนพิเศษ & Normal (เหมือนเดิม) ------------------
                else if (usingShotgun && shotgunAmmo > 0)
                {
                    ShootShotgun(shootPoint, direction, 10f);
                    PlayGunSound(shotgunSound);
                    shotgunAmmo--;
                    nextFireTime = Time.time + shotgunFireRate;
                }
                else if (usingHoming && homingAmmo > 0)
                {
                    Shoot(currentBulletPrefab, shootPoint, direction, 8f);
                    PlayGunSound(homingGunSound);
                    homingAmmo--;
                    nextFireTime = Time.time + 0.4f;
                }
                else if (usingMachineGun && machineGunAmmo > 0)
                {
                    Shoot(currentBulletPrefab, shootPoint, direction, 12f);
                    nextFireTime = Time.time + machineGunFireRate;
                    machineGunAmmo--;
                }
                else if (!usingMachineGun && !usingShotgun && !usingHoming)
                {
                    Shoot(currentBulletPrefab, shootPoint, direction, 10f);
                    PlayGunSound(normalGunSound);
                    nextFireTime = Time.time + normalFireRate;
                }
            }
        }
        // ------------------ Input.GetKey(KeyCode.K) คือ else block ------------------
        else
        {
            if (isMachineGunFiring)
            {
                audioSource.Stop();
                audioSource.loop = false;
                isMachineGunFiring = false;
            }

            if (animator != null) animator.SetBool("isShooting", false);
        }
    }

    Transform GetShootPoint()
    {
        Vector2 dir = playerMove.ShootDirection;

        if (playerMove.IsCrouching)
        {
            if (dir == Vector2.right && crouchFirePointRight != null) return crouchFirePointRight;
            if (dir == Vector2.left && crouchFirePointLeft != null) return crouchFirePointLeft;
            if (dir == Vector2.up && crouchFirePointUp != null) return crouchFirePointUp;
            if (dir == Vector2.down && crouchFirePointDown != null) return crouchFirePointDown;
            return crouchFirePoint;
        }
        else
        {
            if (dir == Vector2.right) return firePointRight;
            if (dir == Vector2.left) return firePointLeft;
            if (dir == Vector2.up) return firePointUp;
            if (dir == Vector2.down) return firePointDown;
        }
        return firePointRight;
    }

    void Shoot(GameObject bulletPrefab, Transform shootPoint, Vector2 direction, float speed)
    {
        if (bulletPrefab == null || shootPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        // ตรวจสอบว่า bullet เป็น MachineGunBullet
        MachineGunBullet mgBullet = bullet.GetComponent<MachineGunBullet>();
        if (mgBullet != null)
        {
            mgBullet.speed = speed;
            mgBullet.Initialize(direction);
        }
        else
        {
            // 🎯 ตรวจสอบ HomingBullet
            HomingBullet hBullet = bullet.GetComponent<HomingBullet>();
            if (hBullet != null)
            {
                hBullet.speed = speed;
                hBullet.Initialize(direction); // ส่งทิศทางเริ่มต้น
            }
            else // ถ้าไม่ใช่ปืนพิเศษ ก็ยิงตรงแบบปกติ
            {
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = direction.normalized * speed;
            }
        }

        CreateMuzzleFlash(direction, shootPoint);
    }


    void ShootShotgun(Transform shootPoint, Vector2 direction, float speed)
    {
        float shotgunSpeed = 20f;
        float lifetime = 0.2f;

        for (int i = -1; i <= 1; i++)
        {
            float angle = 7f * i;
            Vector2 spreadDir = Quaternion.Euler(0, 0, angle) * direction;
            GameObject bullet = Instantiate(shotgunBulletPrefab, shootPoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = spreadDir * shotgunSpeed;
            Destroy(bullet, lifetime);
        }

        CreateMuzzleFlash(direction, shootPoint);
    }

    void CreateMuzzleFlash(Vector2 direction, Transform shootPoint)
    {
        if (muzzleFlashPrefab == null) return;
        if (currentMuzzleFlash != null) Destroy(currentMuzzleFlash);

        currentMuzzleFlash = Instantiate(muzzleFlashPrefab, shootPoint.position, Quaternion.identity);

        if (direction == Vector2.left) currentMuzzleFlash.transform.eulerAngles = new Vector3(0, 180, 0);
        else if (direction == Vector2.up) currentMuzzleFlash.transform.eulerAngles = new Vector3(0, 0, 90);
        else if (direction == Vector2.down) currentMuzzleFlash.transform.eulerAngles = new Vector3(0, 0, -90);
        else currentMuzzleFlash.transform.eulerAngles = Vector3.zero;

        Destroy(currentMuzzleFlash, 0.1f);
    }

    void PlayGunSound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    // -------------------- Switch weapons (ไม่เติม ammo เวลาสลับ) --------------------
    public void SwitchToMachineGun(GameObject newBulletPrefab, int ammoToAdd = 0)
    {
        usingMachineGun = true;
        usingShotgun = false;
        usingHoming = false;
        currentBulletPrefab = newBulletPrefab;
        if (ammoToAdd > 0) machineGunAmmo += ammoToAdd;
    }

    public void SwitchToShotgun(GameObject newBulletPrefab, int ammoToAdd = 0)
    {
        usingShotgun = true;
        usingMachineGun = false;
        usingHoming = false;
        currentBulletPrefab = newBulletPrefab;
        if (ammoToAdd > 0) shotgunAmmo += ammoToAdd;
    }

    public void SwitchToHomingGun(GameObject newBulletPrefab, int ammoToAdd = 0)
    {
        usingHoming = true;
        usingMachineGun = false;
        usingShotgun = false;
        currentBulletPrefab = newBulletPrefab;
        if (ammoToAdd > 0) homingAmmo += ammoToAdd;
    }

    public void SwitchToNormalGun()
    {
        usingMachineGun = false;
        usingShotgun = false;
        usingHoming = false;
        currentBulletPrefab = normalBulletPrefab;
    }

    // Properties สำหรับ UI
    public bool IsUsingMachineGun => usingMachineGun;
    public bool IsUsingShotgun => usingShotgun;
    public bool IsUsingHoming => usingHoming;
    public int MachineGunAmmo => machineGunAmmo;
    public int ShotgunAmmo => shotgunAmmo;
    public int HomingAmmo => homingAmmo;
}
