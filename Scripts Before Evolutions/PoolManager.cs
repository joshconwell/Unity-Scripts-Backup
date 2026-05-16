using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [System.Serializable]
    private class PoolConfig
    {
        public string poolName;
        public GameObject prefab;
        [Min(0)] public int initialSize = 25;
        public bool canExpand = true;
    }

    public static PoolManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("Pool Setup")]
    [SerializeField] private PoolConfig[] poolConfigs;

    [Header("Scene Behavior")]
    [SerializeField] private bool dontDestroyOnLoad = false;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, bool> expansionRules = new Dictionary<GameObject, bool>();
    private readonly Dictionary<GameObject, Transform> poolParents = new Dictionary<GameObject, Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        BuildPools();
    }

    private void BuildPools()
    {
        if (poolConfigs == null)
        {
            return;
        }

        for (int i = 0; i < poolConfigs.Length; i++)
        {
            PoolConfig config = poolConfigs[i];

            if (config == null || config.prefab == null)
            {
                continue;
            }

            CreatePool(config.prefab, config.initialSize, config.canExpand, config.poolName);
        }
    }

    private void CreatePool(GameObject prefab, int initialSize, bool canExpand, string poolName)
    {
        if (prefab == null)
        {
            return;
        }

        if (pools.ContainsKey(prefab))
        {
            return;
        }

        Queue<GameObject> newPool = new Queue<GameObject>();
        pools.Add(prefab, newPool);
        expansionRules.Add(prefab, canExpand);

        GameObject parentObject = new GameObject(string.IsNullOrWhiteSpace(poolName) ? $"{prefab.name} Pool" : poolName);
        parentObject.transform.SetParent(transform);
        poolParents.Add(prefab, parentObject.transform);

        for (int i = 0; i < initialSize; i++)
        {
            GameObject pooledObject = CreatePooledInstance(prefab);
            newPool.Enqueue(pooledObject);
        }
    }

    private GameObject CreatePooledInstance(GameObject prefab)
    {
        GameObject pooledObject = Instantiate(prefab);

        PooledObject pooledComponent = pooledObject.GetComponent<PooledObject>();

        if (pooledComponent == null)
        {
            pooledComponent = pooledObject.AddComponent<PooledObject>();
        }

        pooledComponent.SetOriginalPrefab(prefab);

        if (poolParents.ContainsKey(prefab))
        {
            pooledObject.transform.SetParent(poolParents[prefab]);
        }

        pooledObject.SetActive(false);

        return pooledObject;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogWarning("PoolManager tried to spawn a null prefab.");
            return null;
        }

        if (!pools.ContainsKey(prefab))
        {
            CreatePool(prefab, 0, true, $"{prefab.name} Pool");
        }

        Queue<GameObject> pool = pools[prefab];

        GameObject objectToSpawn = null;

        if (pool.Count > 0)
        {
            objectToSpawn = pool.Dequeue();
        }
        else
        {
            bool canExpand = true;

            if (expansionRules.ContainsKey(prefab))
            {
                canExpand = expansionRules[prefab];
            }

            if (canExpand)
            {
                objectToSpawn = CreatePooledInstance(prefab);
            }
            else
            {
                Debug.LogWarning($"Pool for {prefab.name} is empty and cannot expand.");
                return null;
            }
        }

        objectToSpawn.transform.SetPositionAndRotation(position, rotation);
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject objectToReturn)
    {
        if (objectToReturn == null)
        {
            return;
        }

        PooledObject pooledObject = objectToReturn.GetComponent<PooledObject>();

        if (pooledObject == null || pooledObject.OriginalPrefab == null)
        {
            Destroy(objectToReturn);
            return;
        }

        GameObject originalPrefab = pooledObject.OriginalPrefab;

        if (!pools.ContainsKey(originalPrefab))
        {
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);

        if (poolParents.ContainsKey(originalPrefab))
        {
            objectToReturn.transform.SetParent(poolParents[originalPrefab]);
        }

        pools[originalPrefab].Enqueue(objectToReturn);
    }
}