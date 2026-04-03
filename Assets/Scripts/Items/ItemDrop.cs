using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private ItemDropData _data;
    [SerializeField] private Renderer[] _renderers;

    private Transform _player;
    private PlayerStats _playerStats;
    private float _spawnTime;

    [Header("Magnet")]
    [SerializeField] private float _detectRadius = 4f;
    [SerializeField] private float _magnetSpeed = 5f;
    [SerializeField] private float _collectRadius = 0.6f;

    [Header("Rotation")]
    [SerializeField] private float _rotateSpeed = 90f;

    private bool _isMovingToPlayer;
    private bool _collected;

    private void Start()
    {
        _spawnTime = Time.time;

        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            _player = go.transform;
            _playerStats = go.GetComponent<PlayerStats>();
        }

        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<Renderer>();

        StartCoroutine(CoLifetime());
    }

    private void Update()
    {
        if (_collected || _player == null) return;

        transform.Rotate(0f, _rotateSpeed * Time.deltaTime, 0f);

        float dist = Vector3.Distance(transform.position, _player.position);

        // 감지 범위 안이면 자석 이동
        if (dist <= _detectRadius)
        {
            _isMovingToPlayer = true;
        }
        else
        {
            _isMovingToPlayer = false;
        }

        if (_isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _player.position,
                _magnetSpeed * Time.deltaTime);

            // 수집
            if (dist <= _collectRadius)
                Collect();
        }
    }

    public void Init(ItemDropData data)
    {
        _data = data;
    }

    private void Collect()
    {
        if (_collected) return;
        _collected = true;

        switch (_data.dropType)
        {
            case ItemDropType.AmmoAR:
                SFXManager.PlaySound(SoundType.ItemPickup_Ammo);
                CollectAmmoAR();
                break;
            case ItemDropType.AmmoShotgun:
                SFXManager.PlaySound(SoundType.ItemPickup_Ammo);
                CollectAmmoShotgun();
                break;
            case ItemDropType.EnergyDrink:
                SFXManager.PlaySound(SoundType.ItemPickup_EnergyDrink);
                CollectEnergyDrink();
                break;
            case ItemDropType.MedKit:
                SFXManager.PlaySound(SoundType.ItemPickup_MedKit);
                CollectMedKit();
                break;
            case ItemDropType.Gold:
                SFXManager.PlaySound(SoundType.ItemPickup_Gold);
                CollectGold();
                break;
        }

        Destroy(gameObject);
    }

    private void CollectAmmoAR()
    {
        // AR GunController 찾아서 예비탄약 채우기
        var guns = _player.GetComponentsInChildren<GunController>(true);
        foreach (var gun in guns)
        {
            if (gun._data != null && gun._data.weaponName == "Assault Rifle")
            {
                gun.FillReserveAmmo(_data.ammoAmount);
                break;
            }
        }
    }

    private void CollectAmmoShotgun()
    {
        var guns = _player.GetComponentsInChildren<GunController>(true);
        foreach (var gun in guns)
        {
            if (gun._data != null && gun._data.weaponName == "Pump Shotgun")
            {
                gun.FillReserveAmmo(_data.ammoAmount);
                break;
            }
        }
    }

    private void CollectEnergyDrink()
    {
        if (_playerStats != null)
            _playerStats.ApplyStaminaBoost(_data.staminaRegenMultiplier, _data.staminaBoostDuration);
    }

    private void CollectMedKit()
    {
        if (_playerStats != null)
            _playerStats.Heal(_playerStats._maxHp * _data.healPercent);
    }

    private void CollectGold()
    {
        if (GoldManager.Instance == null) return;
        int amount = Random.Range(_data.goldMin, _data.goldMax + 1);
        GoldManager.Instance.Add(amount, transform.position);
    }

    private IEnumerator CoLifetime()
    {
        // 일반 대기
        yield return new WaitForSeconds(_data.blinkStartTime);

        // 깜빡임
        float blinkDuration = _data.lifetime - _data.blinkStartTime;
        float blinkSpeed = 8f;
        float t = 0f;

        while (t < blinkDuration)
        {
            t += Time.deltaTime;
            bool visible = Mathf.Sin(t * blinkSpeed) > 0f;
            foreach (var r in _renderers)
                if (r != null) r.enabled = visible;
            yield return null;
        }

        if (!_collected) Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _collectRadius);
    }
}