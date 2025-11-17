using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        [Range(0, 100)] public float dropRatePercent;
    }

    public DropItem[] dropTable;

    public void Drop()
    {
        if (dropTable.Length == 0) return;

        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (DropItem item in dropTable)
        {
            cumulative += item.dropRatePercent;
            if (roll <= cumulative)
            {
                GameObject drop = Instantiate(item.itemPrefab, transform.position, Quaternion.identity);

                Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(2f, 4f));

                return;
            }
        }
    }
}
