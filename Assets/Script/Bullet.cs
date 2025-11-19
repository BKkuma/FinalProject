using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // บอสเก่า
            HelicopterBoss boss = collision.gameObject.GetComponent<HelicopterBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            // ** บอส Phase 2 ที่คุณต้องการเพิ่ม **
            Boss2 boss2 = collision.gameObject.GetComponent<Boss2>();
            if (boss2 != null)
            {
                boss2.TakeDamage(damage);
            }

            // สำหรับศัตรูธรรมดา
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            EnemyFlyingAI flyingEnemy = collision.gameObject.GetComponent<EnemyFlyingAI>();
            if (flyingEnemy != null)
            {
                flyingEnemy.TakeDamage(damage);
            }
        }

        Destroy(gameObject); // กระสุนหายเมื่อชน
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}