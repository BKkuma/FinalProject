using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Switch/Pickup Sounds")]
    public AudioClip machineGunPickupSound;
    public AudioClip shotgunPickupSound;
    public AudioClip homingGunPickupSound;
    public AudioClip switchSound;

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
        bool isSpecialGunEmpty = false;

        if (Input.GetKey(KeyCode.K))
        {
            if (animator != null) animator.SetBool("isShooting", true);

           
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
                else 
                {
                    if (isMachineGunFiring)
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                        isMachineGunFiring = false;
                    }
                    isSpecialGunEmpty = true;
                }
            }

            // ------------------ Logic การยิงจริง ------------------
            if (Time.time >= nextFireTime)
            {
                
                Transform shootPoint = GetShootPoint();
                Vector2 direction = playerMove.ShootDirection;

                if (usingShotgun && shotgunAmmo <= 0)
                {
                    isSpecialGunEmpty = true;
                }
                else if (usingHoming && homingAmmo <= 0)
                {
                    isSpecialGunEmpty = true;
                }

                if (isSpecialGunEmpty)
                {
                    if (ammoEmptySound != null && audioSource.clip != ammoEmptySound)
                    {
                        PlayGunSound(ammoEmptySound);
                        nextFireTime = Time.time + normalFireRate;
                    }
                }
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

        MachineGunBullet mgBullet = bullet.GetComponent<MachineGunBullet>();
        if (mgBullet != null)
        {
            mgBullet.speed = speed;
            mgBullet.Initialize(direction);
        }
        else
        {
            HomingBullet hBullet = bullet.GetComponent<HomingBullet>();
            if (hBullet != null)
            {
                hBullet.speed = speed;
                hBullet.Initialize(direction);
            }
            else
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

    // -------------------- ฟังก์ชันสำหรับเก็บปืน/เติมกระสุน --------------------
    public void PickupAmmo(string gunType, int ammoToAdd)
    {
        AudioClip pickupSound = null;

        switch (gunType)
        {
            case "MachineGun":
                machineGunAmmo += ammoToAdd;
                pickupSound = machineGunPickupSound;
                break;
            case "Shotgun":
                shotgunAmmo += ammoToAdd;
                pickupSound = shotgunPickupSound;
                break;
            case "HomingGun":
                homingAmmo += ammoToAdd;
                pickupSound = homingGunPickupSound;
                break;
            default:
                Debug.LogWarning("Unknown gun type in PickupAmmo: " + gunType);
                return;
        }

        if (pickupSound != null)
        {
            // เล่นเสียงเก็บปืน/เสียงพูด
            audioSource.PlayOneShot(pickupSound);
            Debug.Log($"Played Pickup Sound for {gunType}");
        }
        else
        {
            Debug.LogWarning($"Pickup Sound for {gunType} is missing!");
        }
    }


    // -------------------- Switch weapons --------------------
    public void SwitchToMachineGun(GameObject newBulletPrefab)
    {
        if (!usingMachineGun) PlayGunSound(switchSound);

        usingMachineGun = true;
        usingShotgun = false;
        usingHoming = false;
        currentBulletPrefab = newBulletPrefab;
    }

    public void SwitchToShotgun(GameObject newBulletPrefab)
    {
        if (!usingShotgun) PlayGunSound(switchSound);

        usingShotgun = true;
        usingMachineGun = false;
        usingHoming = false;
        currentBulletPrefab = newBulletPrefab;
    }

    public void SwitchToHomingGun(GameObject newBulletPrefab)
    {
        if (!usingHoming) PlayGunSound(switchSound);

        usingHoming = true;
        usingMachineGun = false;
        usingShotgun = false;
        currentBulletPrefab = newBulletPrefab;
    }

    public void SwitchToNormalGun()
    {
        if (usingMachineGun || usingShotgun || usingHoming)
        {
            PlayGunSound(switchSound);
        }

        usingMachineGun = false;
        usingShotgun = false;
        usingHoming = false;
        currentBulletPrefab = normalBulletPrefab;
    }

    
    public bool IsUsingMachineGun => usingMachineGun;
    public bool IsUsingShotgun => usingShotgun;
    public bool IsUsingHoming => usingHoming;
    public int MachineGunAmmo => machineGunAmmo;
    public int ShotgunAmmo => shotgunAmmo;
    public int HomingAmmo => homingAmmo;
}