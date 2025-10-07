using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Fire Settings")]
    public Transform firePointRight;
    public Transform firePointLeft;
    public Transform firePointUp;
    public Transform firePointDown;
    public Transform crouchFirePoint;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlashPrefab;
    private GameObject currentMuzzleFlash;

    [Header("Bullet Prefabs")]
    public GameObject normalBulletPrefab;
    public GameObject machineGunBulletPrefab;
    public GameObject shotgunBulletPrefab;

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

    private bool usingMachineGun = false;
    private bool usingShotgun = false;
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
        if (!Input.GetKey(KeyCode.K))
        {
            if (animator != null) animator.SetBool("isShooting", false);
            return;
        }

        if (animator != null) animator.SetBool("isShooting", true);

        if (Time.time < nextFireTime) return;

        Transform shootPoint = GetShootPoint();
        Vector2 direction = playerMove.ShootDirection;

        if (usingShotgun)
        {
            ShootShotgun(shootPoint, direction, 10f);
            shotgunAmmo--;
            if (shotgunAmmo <= 0)
            {
                usingShotgun = false;
                currentBulletPrefab = normalBulletPrefab;
                Debug.Log("🔁 หมดกระสุนลูกซอง! กลับไปปืนปกติ");
            }
            nextFireTime = Time.time + shotgunFireRate;
        }
        else if (usingMachineGun)
        {
            Shoot(currentBulletPrefab, shootPoint, direction, 12f);
            machineGunAmmo--;
            if (machineGunAmmo <= 0)
            {
                usingMachineGun = false;
                currentBulletPrefab = normalBulletPrefab;
                Debug.Log("🔁 หมดกระสุนปืนกล! กลับไปปืนปกติ");
            }
            nextFireTime = Time.time + machineGunFireRate;
        }
        else
        {
            Shoot(currentBulletPrefab, shootPoint, direction, 10f);
            nextFireTime = Time.time + normalFireRate;
        }
    }

    Transform GetShootPoint()
    {
        if (playerMove.IsCrouching && crouchFirePoint != null)
            return crouchFirePoint;

        Vector2 dir = playerMove.ShootDirection;

        if (dir == Vector2.right) return firePointRight;
        else if (dir == Vector2.left) return firePointLeft;
        else if (dir == Vector2.up) return firePointUp;
        else if (dir == Vector2.down) return firePointDown;

        return firePointRight;
    }

    void Shoot(GameObject bulletPrefab, Transform shootPoint, Vector2 direction, float speed)
    {
        if (bulletPrefab == null || shootPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = direction * speed;

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
}
