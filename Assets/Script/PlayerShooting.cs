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
    public GameObject muzzleFlashPrefab; // prefab ของ effect
    private GameObject currentMuzzleFlash;


    public GameObject normalBulletPrefab;
    public GameObject machineGunBulletPrefab;
    public float normalFireRate = 0.25f;
    public float machineGunFireRate = 0.1f;

    private float nextFireTime = 0f;
    private PlayerMovement playerMove;
    private Animator animator;

    [Header("Machine Gun Settings")]
    public int machineGunAmmo = 50;
    private bool usingMachineGun = false;
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

                Shoot(currentBulletPrefab, shootPoint, direction, usingMachineGun ? 12f : 10f);

                if (usingMachineGun)
                {
                    machineGunAmmo--;
                    if (machineGunAmmo <= 0)
                    {
                        usingMachineGun = false;
                        currentBulletPrefab = normalBulletPrefab;
                        Debug.Log("🔁 หมดกระสุนปืนกล! กลับมาใช้ปืนปกติ");
                    }
                    nextFireTime = Time.time + machineGunFireRate;
                }
                else
                {
                    nextFireTime = Time.time + normalFireRate;
                }
            }
        }
        else
        {
            if (animator != null) animator.SetBool("isShooting", false);
        }
    }

    Transform GetShootPoint()
    {
        // ถ้า crouch ใช้ crouchFirePoint
        if (playerMove.IsCrouching && crouchFirePoint != null)
            return crouchFirePoint;

        Vector2 dir = playerMove.ShootDirection;

        if (dir == Vector2.right) return firePointRight;
        else if (dir == Vector2.left) return firePointLeft;
        else if (dir == Vector2.up) return firePointUp;
        else if (dir == Vector2.down) return firePointDown;

        return firePointRight; // default
    }

    void Shoot(GameObject bulletPrefab, Transform shootPoint, Vector2 direction, float speed)
    {
        if (bulletPrefab == null || shootPoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = direction * speed;

        if (muzzleFlashPrefab != null)
        {
            // ทำให้หมุน/flip ตามทิศทาง
            if (currentMuzzleFlash != null) Destroy(currentMuzzleFlash);
            currentMuzzleFlash = Instantiate(muzzleFlashPrefab, shootPoint.position, Quaternion.identity);

            // หมุน effect ตามทิศทางยิง
            if (direction == Vector2.left) currentMuzzleFlash.transform.eulerAngles = new Vector3(0, 180, 0);
            else if (direction == Vector2.up) currentMuzzleFlash.transform.eulerAngles = new Vector3(0, 0, 90);
            else if (direction == Vector2.down) currentMuzzleFlash.transform.eulerAngles = new Vector3(0, 0, -90);
            else currentMuzzleFlash.transform.eulerAngles = Vector3.zero;

            // ปิดหลัง 0.1 วิ
            Destroy(currentMuzzleFlash, 0.1f);
        }
    }

    public void PickupMachineGun(int ammoAmount, GameObject newBulletPrefab)
    {
        usingMachineGun = true;
        machineGunAmmo = ammoAmount;
        currentBulletPrefab = newBulletPrefab;

        Debug.Log($"💥 ได้ปืนกล! กระสุน {ammoAmount} นัด");
    }
}
