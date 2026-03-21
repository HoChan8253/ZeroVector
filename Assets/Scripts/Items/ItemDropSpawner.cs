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
        TrySpawnByChance(_gold, position);

        // AR 탄약 - 언락 시에만
        if (_arUpgradeManager != null && _arUpgradeManager.IsOwned)
            TrySpawnByChance(_ammoAR, position);

        // 샷건 탄약 - 언락 시에만
        if (_shotgunUpgradeManager != null && _shotgunUpgradeManager.IsOwned)
            TrySpawnByChance(_ammoShotgun, position);

        // 에너지 드링크
        TrySpawnByChance(_energyDrink, position);

        // 구급 상자
        TrySpawnByChance(_medKit, position);
    }

    private void TrySpawnByChance(ItemDropData data, Vector3 position)
    {
        if (data == null) return;
        if (Random.value <= data.dropChance)
            SpawnItem(data, position);
    }

    private void SpawnItem(ItemDropData data, Vector3 position)
    {
        if (data?.prefab == null) return;
        Vector3 spawnPos = position + Vector3.up * 0.5f
            + new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
        var go = Instantiate(data.prefab, spawnPos, Quaternion.identity);
        var behaviour = go.GetComponent<ItemDrop>();
        if (behaviour != null)
            behaviour.Init(data);
    }
}