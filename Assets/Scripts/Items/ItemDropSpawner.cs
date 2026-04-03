using UnityEngine;

public class ItemDropSpawner : MonoBehaviour
{
    [Header("드랍 테이블")]
    [SerializeField] private ItemDropData _ammoAR;
    [SerializeField] private ItemDropData _ammoShotgun;
    [SerializeField] private ItemDropData _energyDrink;
    [SerializeField] private ItemDropData _medKit;
    [SerializeField] private ItemDropData _gold;

    [Header("무기 언락 매니저")]
    [SerializeField] private WeaponUpgradeManager _arUpgradeManager;
    [SerializeField] private WeaponUpgradeManager _shotgunUpgradeManager;

    public static ItemDropSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TrySpawn(Vector3 position)
    {
        // 드랍 후보 목록 우선순위 순서
        ItemDropData[] candidates = new ItemDropData[]
        {
        _medKit,
        _ammoAR,
        _ammoShotgun,
        _energyDrink,
        _gold
        };

        // 후보 중 하나만 드랍
        foreach (var data in candidates)
        {
            if (data == null) continue;

            if (data == _ammoAR && (_arUpgradeManager == null || !_arUpgradeManager.IsOwned)) continue;
            if (data == _ammoShotgun && (_shotgunUpgradeManager == null || !_shotgunUpgradeManager.IsOwned)) continue;

            if (Random.value <= data.dropChance)
            {
                SpawnItem(data, position);
                return;
            }
        }
    }

    private void SpawnItem(ItemDropData data, Vector3 position)
    {
        if (data?.prefab == null) return;

        Vector3 groundPos = position;
        if (Physics.Raycast(position, Vector3.down, out var hit, 50f))
            groundPos = hit.point;

        Vector3 spawnPos = groundPos + Vector3.up * 0.5f
            + new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

        var go = Instantiate(data.prefab, spawnPos, Quaternion.identity);
        var behaviour = go.GetComponent<ItemDrop>();
        if (behaviour != null)
            behaviour.Init(data);
    }
}