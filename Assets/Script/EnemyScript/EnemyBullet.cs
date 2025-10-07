using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Hit!");
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            // ชนพื้น/กำแพงก็ดับ
            Destroy(gameObject);
        }
    }
}
