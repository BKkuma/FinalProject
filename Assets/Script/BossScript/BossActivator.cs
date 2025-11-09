using UnityEngine;

public class BossActivator : MonoBehaviour
{
    public GameObject boss;
    public Camera mainCamera;
    public Camera bossCamera;
    public GameObject player;

    [Header("Boss Boundaries")]
    public GameObject bossBounds; // ✅ เพิ่มช่องอ้างอิงกำแพง

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // ✅ เปิด Boss
            if (boss != null)
                boss.SetActive(true);

            // ✅ ปิด PlayerBounds ชั่วคราว
            PlayerBounds pb = player.GetComponent<PlayerBounds>();
            if (pb != null)
                pb.enabled = false;

            // ✅ สลับกล้องไป BossCamera
            if (mainCamera != null && bossCamera != null)
            {
                mainCamera.enabled = false;
                bossCamera.enabled = true;
            }

            // ✅ เปิดกำแพงบอส
            if (bossBounds != null)
                bossBounds.SetActive(true);

            // ✅ เปลี่ยนเพลงเป็นเพลงบอส
            if (MusicManager.Instance != null)
                MusicManager.Instance.PlayBossMusic();

            // ✅ สมัคร Event ตอนบอสตาย
            HelicopterBoss heli = boss.GetComponent<HelicopterBoss>();
            if (heli != null)
            {
                heli.onBossDefeated += () =>
                {
                    // 🔁 กลับ MainCamera
                    if (mainCamera != null && bossCamera != null)
                    {
                        mainCamera.enabled = true;
                        bossCamera.enabled = false;
                    }

                    // 🔁 เปิด PlayerBounds กลับมา
                    if (pb != null)
                        pb.enabled = true;

                    // 🔁 ปิดกำแพงบอส
                    if (bossBounds != null)
                        bossBounds.SetActive(false);

                    // 🔊 กลับไปเพลงปกติ
                    if (MusicManager.Instance != null)
                        MusicManager.Instance.PlayNormalMusic();
                };
            }

            // ✅ ปิด Trigger เพื่อไม่ให้เรียกซ้ำ
            gameObject.SetActive(false);
        }
    }
}
