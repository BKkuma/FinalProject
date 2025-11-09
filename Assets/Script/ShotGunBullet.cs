using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunBullet : MonoBehaviour
{
    public int damage = 1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HelicopterBoss boss = collision.gameObject.GetComponent<HelicopterBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
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
