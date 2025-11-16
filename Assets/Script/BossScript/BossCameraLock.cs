using UnityEngine;

public class BossCameraLock : MonoBehaviour
{
    private Vector2 minLimit;
    private Vector2 maxLimit;

    public void Init(Vector2 min, Vector2 max)
    {
        minLimit = min;
        maxLimit = max;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minLimit.x, maxLimit.x);
        pos.y = Mathf.Clamp(pos.y, minLimit.y, maxLimit.y);
        transform.position = pos;
    }
}
