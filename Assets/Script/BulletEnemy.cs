using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 5f;

    private Vector2 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void ShootTowards(Vector2 targetPosition)
    {
        moveDirection = (targetPosition - (Vector2)transform.position).normalized * speed;
        GetComponent<Rigidbody2D>().velocity = moveDirection;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player โดนกระสุน Enemy!");
            // TODO: เรียก PlayerHealth.TakeDamage() ที่นี่
        }
        Destroy(gameObject);
    }
}
