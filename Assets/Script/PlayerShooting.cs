using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Fire Settings")]
    public Transform firePointRight;
    public Transform firePointLeft;
    public Transform firePointUp;
    public Transform firePointDown;
    public Transform crouchFirePoint;

    [Header("Crouch Fire Points")]
    public Transform crouchFirePointRight;
    public Transform crouchFirePointLeft;
    public Transform crouchFirePointUp;
    public Transform crouchFirePointDown;


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
    public int machineGunAmmo = 50;
    public int shotgunAmmo = 10;
    public int homingAmmo = 15;

    private bool usingMachineGun = false;
    private bool usingShotgun = false;
    private bool usingHoming = false;
    private GameObject currentBulletPrefab;

    void Start()
    {
        playerMove = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        currentBulletPrefab = normalBulletPrefab;
    }

    void Update()
    {
        HandleShoot();
    }

    void HandleShoot()
    {
        if (Input.GetKey(KeyCode.K))
        {
            if (animator != null) animator.SetBool("isShooting", true);

            if (Time.time >= nextFireTime)
            {
                Transform shootPoint = GetShootPoint();
                Vector2 direction = playerMove.ShootDirection;

                if (usingShotgun)
                {
                    ShootShotgun(shootPoint, direction, 10f);
                    shotgunAmmo--;
                    if (shotgunAmmo <= 0) ResetToNormalGun();
                    nextFireTime = Time.time + shotgunFireRate;
                }
                else if (usingHoming)
                {
                    Shoot(currentBulletPrefab, shootPoint, direction, 8f);
                    homingAmmo--;
                    if (homingAmmo <= 0) ResetToNormalGun();
                    nextFireTime = Time.time + 0.4f;
                }
                else
                {
                    Shoot(currentBulletPrefab, shootPoint, direction, usingMachineGun ? 12f : 10f);
                    if (usingMachineGun)
                    {
                        machineGunAmmo--;
                        if (machineGunAmmo <= 0) ResetToNormalGun();
                        nextFireTime = Time.time + machineGunFireRate;
                    }
                    else
                    {
                        nextFireTime = Time.time + normalFireRate;
                    }
                }
            }
        }
        else if (animator != null) animator.SetBool("isShooting", false);
    }

    Transform GetShootPoint()
    {
        Vector2 dir = playerMove.ShootDirection;

        if (playerMove.IsCrouching)
        {
            if (dir == Vector2.right && crouchFirePointRight != null) return crouchFirePointRight;
            else if (dir == Vector2.left && crouchFirePointLeft != null) return crouchFirePointLeft;
            else if (dir == Vector2.up && crouchFirePointUp != null) return crouchFirePointUp;
            else if (dir == Vector2.down && crouchFirePointDown != null) return crouchFirePointDown;

            // fallback
            if (crouchFirePoint != null) return crouchFirePoint;
        }
        else
        {
            if (dir == Vector2.right) return firePointRight;
            else if (dir == Vector2.left) return firePointLeft;
            else if (dir == Vector2.up) return firePointUp;
            else if (dir == Vector2.down) return firePointDown;
        }

        return firePointRight;
    }


    void Shoot(GameObject bulletPrefab, Transform shootPoint, Vector2 direction, float speed)
    {
        if (bulletPrefab == null || shootPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        // ✅ ถ้าเป็น HomingBullet ให้กำหนด initialDirection
        HomingBullet hb = bullet.GetComponent<HomingBullet>();
        if (hb != null)
        {
            hb.initialDirection = direction.normalized;
        }
        else
        {
            // ✅ ถ้าเป็น MachineGunBullet ให้กำหนด direction
            MachineGunBullet mg = bullet.GetComponent<MachineGunBullet>();
            if (mg != null)
            {
                mg.direction = direction.normalized;

                // หมุนหัวกระสุนให้ชี้ไปทิศทางจริง
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = direction.normalized * speed;
        }

        CreateMuzzleFlash(direction, shootPoint);
    }



    void ShootShotgun(Transform shootPoint, Vector2 direction, float speed)
    {
        for (int i = -1; i <= 1; i++)
        {
            float angle = 7f * i;
            Vector2 spreadDir = Quaternion.Euler(0, 0, angle) * direction;
            GameObject bullet = Instantiate(shotgunBulletPrefab, shootPoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = spreadDir * speed;
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
    public void ResetToNormalGun()
    {
        usingMachineGun = usingShotgun = usingHoming = false;
        currentBulletPrefab = normalBulletPrefab;
        Debug.Log("🔁 หมดกระสุน! กลับมาใช้ปืนปกติ");
    }

    public void PickupMachineGun(int ammoAmount, GameObject newBulletPrefab)
    {
        usingMachineGun = true;
        usingShotgun = false;
        machineGunAmmo = ammoAmount;
        currentBulletPrefab = newBulletPrefab;

        Debug.Log($"💥 ได้ปืนกล! กระสุน {ammoAmount} นัด");
    }

    public void PickupShotgun(int ammoAmount, GameObject newBulletPrefab)
    {
        usingShotgun = true;
        usingMachineGun = false;
        shotgunAmmo = ammoAmount;
        currentBulletPrefab = newBulletPrefab;

        Debug.Log($"💥 ได้ปืนลูกซอง! กระสุน {ammoAmount} นัด");
    }
    public void PickupHomingGun(int ammoAmount, GameObject newBulletPrefab)
    {
        usingHoming = true;
        usingMachineGun = false;
        usingShotgun = false;
        homingAmmo = ammoAmount;
        currentBulletPrefab = newBulletPrefab;
        Debug.Log($"🎯 ได้ปืนติดตาม! กระสุน {ammoAmount} นัด");
    }


    // วางไว้ท้าย class PlayerShooting
    public bool IsUsingMachineGun => usingMachineGun;
    public bool IsUsingShotgun => usingShotgun;
    public bool IsUsingHoming => usingHoming;

    public int MachineGunAmmo => machineGunAmmo;
    public int ShotgunAmmo => shotgunAmmo;
    public int HomingAmmo => homingAmmo;

}
