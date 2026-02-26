using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            _pools.Add(prefab, q);
        }

        GameObject obj = null;
        while (q.Count > 0 && obj == null)
            obj = q.Dequeue();

        if (obj == null)
            obj = Instantiate(prefab);

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);
        return obj;
    }

    public void Despawn(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null) return;

        if (!_pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            _pools.Add(prefab, q);
        }

        obj.SetActive(false);
        q.Enqueue(obj);
    }
}