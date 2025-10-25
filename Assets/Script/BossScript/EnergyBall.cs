using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    public float fallSpeed = 8f;
    public int damage = 10;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // �������������Ǣ����硹���
        rb.velocity = Vector2.down * fallSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by energy ball!");
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
