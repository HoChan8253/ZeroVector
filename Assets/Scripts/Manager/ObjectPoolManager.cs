using UnityEngine;
using System.Collections.Generic;

public enum PoolKey
{
    EnergyBall,
    HitFx_ElectricShort,
    BossProj_Straight,
    BossProj_Homing
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    [Header("Prefab Registry")]
    [SerializeField] private GameObject _energyBallPrefab;
    [SerializeField] private GameObject _hitFxElectricShortPrefab;
    [SerializeField] private GameObject _bossProjStraightPrefab;
    [SerializeField] private GameObject _bossProjHomingPrefab;

    private readonly Dictionary<PoolKey, GameObject> _prefabs = new();
    private readonly Dictionary<PoolKey, Queue<GameObject>> _pools = new();

    [Header("Pool Root")]
    [SerializeField] private Transform _poolRoot;
    private readonly Dictionary<PoolKey, Transform> _poolParents = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_poolRoot == null)
        {
            var go = new GameObject("PoolRoot");
            _poolRoot = go.transform;
        }

        _prefabs[PoolKey.EnergyBall] = _energyBallPrefab;
        _prefabs[PoolKey.HitFx_ElectricShort] = _hitFxElectricShortPrefab;
        _prefabs[PoolKey.BossProj_Straight] = _bossProjStraightPrefab;
        _prefabs[PoolKey.BossProj_Homing] = _bossProjHomingPrefab;
    }

    private Transform GetPoolParent(PoolKey key)
    {
        if (_poolParents.TryGetValue(key, out var parent) && parent != null)
            return parent;

        var go = new GameObject($"Pool_{key}");
        go.transform.SetParent(_poolRoot, false);
        parent = go.transform;

        _poolParents[key] = parent;
        return parent;
    }

    public void Preload(PoolKey key, int count)
    {
        if (count <= 0) return;
        if (!_prefabs.TryGetValue(key, out var prefab) || prefab == null) return;

        if (!_pools.TryGetValue(key, out var q))
        {
            q = new Queue<GameObject>();
            _pools.Add(key, q);
        }

        var parent = GetPoolParent(key);

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(parent, false);
            q.Enqueue(obj);
        }
    }

    public GameObject Spawn(PoolKey key, Vector3 pos, Quaternion rot)
    {
        if (!_prefabs.TryGetValue(key, out var prefab) || prefab == null)
            return null;

        if (!_pools.TryGetValue(key, out var q))
        {
            q = new Queue<GameObject>();
            _pools.Add(key, q);
        }

        GameObject obj = null;
        while (q.Count > 0 && obj == null)
            obj = q.Dequeue();

        if (obj == null)
        {
            obj = Instantiate(prefab);
        }

        obj.transform.SetParent(null, true);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);
        return obj;
    }

    public void Despawn(PoolKey key, GameObject obj)
    {
        if (obj == null) return;

        if (!_pools.TryGetValue(key, out var q))
        {
            q = new Queue<GameObject>();
            _pools.Add(key, q);
        }

        obj.SetActive(false);
        obj.transform.SetParent(GetPoolParent(key), false);
        q.Enqueue(obj);
    }
}