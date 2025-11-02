using UnityEngine;

public class BossTransformEvent : MonoBehaviour
{
    public GameObject energyOrbPrefab;
    public Transform spawnPoint;
    public Animator bossAnimator;

    private bool orbSpawned = false; // <- เพิ่มตัวนี้

    public void SpawnEnergyOrb()
    {
        if (orbSpawned) return; // ถ้า spawn แล้ว จะไม่ spawn อีก

        if (energyOrbPrefab == null || spawnPoint == null)
        {
            Debug.LogError("EnergyOrb prefab หรือ spawnPoint ยังไม่ได้ assign!");
            return;
        }

        GameObject orb = Instantiate(energyOrbPrefab, spawnPoint.position, Quaternion.identity);

        EnergyOrb energyOrb = orb.GetComponent<EnergyOrb>();
        if (energyOrb != null)
        {
            energyOrb.bossAnimator = bossAnimator;
        }

        orbSpawned = true; // <- mark ว่า spawn แล้ว
        Debug.Log("Spawned EnergyOrb at " + spawnPoint.position);
    }
}
