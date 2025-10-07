using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Camera Settings")]
    public CameraBossLock cameraLock;    // ��ҧ�ԧ���ͧ
    public Vector2 cameraMinLimit;       // �ͺ����-��ҧ
    public Vector2 cameraMaxLimit;       // �ͺ���-��

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // Spawn ���
            Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

            // Lock ���ͧ
            if (cameraLock != null)
            {
                cameraLock.StartBossFight(cameraMinLimit, cameraMaxLimit);
            }

            // �Դ Trigger
            gameObject.SetActive(false);
        }
    }
}
