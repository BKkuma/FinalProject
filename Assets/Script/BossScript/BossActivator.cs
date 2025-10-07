using UnityEngine;

public class BossActivator : MonoBehaviour
{
    public GameObject boss;
    public Camera mainCamera;
    public Camera bossCamera;
    public GameObject player;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // เปิด Boss
            if (boss != null)
                boss.SetActive(true);

            // ปิด PlayerBounds ชั่วคราว
            PlayerBounds pb = player.GetComponent<PlayerBounds>();
            if (pb != null)
                pb.enabled = false;

            // สลับกล้องไป BossCamera
            if (mainCamera != null && bossCamera != null)
            {
                mainCamera.enabled = false;
                bossCamera.enabled = true;
            }

            // สมัคร Event Boss ตาย
            HelicopterBoss heli = boss.GetComponent<HelicopterBoss>();
            if (heli != null)
            {
                heli.onBossDefeated += () =>
                {
                    // กลับ MainCamera
                    if (mainCamera != null && bossCamera != null)
                    {
                        mainCamera.enabled = true;
                        bossCamera.enabled = false;
                    }

                    // เปิด PlayerBounds กลับมา
                    if (pb != null)
                        pb.enabled = true;
                };
            }

            // ปิด Trigger
            gameObject.SetActive(false);
        }
    }
}
