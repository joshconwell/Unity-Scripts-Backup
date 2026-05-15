using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject OriginalPrefab { get; private set; }

    public void SetOriginalPrefab(GameObject originalPrefab)
    {
        OriginalPrefab = originalPrefab;
    }

    public void ReturnToPool()
    {
        if (PoolManager.HasInstance && OriginalPrefab != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}