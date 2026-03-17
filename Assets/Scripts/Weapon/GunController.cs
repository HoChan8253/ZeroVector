using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunController : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData _data;

    [Header("Upgrade")]
    [SerializeField] private WeaponUpgradeManager _upgradeManager;

    [Header("ADS Multipliers")]
    [SerializeField] private float _adsRecoilMult = 0.4f; // 반동% 감소
    [SerializeField] private float _adsSpreadMult = 0.3f; // 탄퍼짐% 감소

    [Header("Air Spread")]
    [SerializeField] private PlayerMoveCC _playerMove;
    [SerializeField] private float _airSpreadMultiplier = 3f;

    [Header("Refs")]
    [SerializeField] private Camera _cam;
    [SerializeField] private Animator _anim;
    [SerializeField] private PlayerInputHub _input;
    [SerializeField] private PlayerLook _look;

    [SerializeField] private ParticleSystem _muzzleFx;
    [SerializeField] private Light _muzzleLight;
    [SerializeField] private float _muzzleLightDuration = 0.1f;

    [SerializeField] private GameObject _hitSparkPrefab;
    [SerializeField] private GameObject _bulletHolePrefab;
    [SerializeField] private int _maxBulletHoles = 50;

    [SerializeField] private PlayerStats _stats;

    private readonly Queue<GameObject> _bulletHoles = new Queue<GameObject>();
    private Coroutine _muzzleLightCo;

    public event Action<int, int> OnAmmoChanged;

    [SerializeField] private bool _pumpLocked;

    [SerializeField] private WeaponState _state = new WeaponState();

    // 캐시
    private float _fireRate;
    private float _range;
    private float _damage;
    private int _magSize;
    private float _reloadDuration;
    private float _recoilPitchAmount;
    private float _recoilYawAmount;
    private float _spreadAmount;
    private float _maxSpread;
    private float _spreadRecoverSpeed;
    private float _tapResetTime;
    private float _moveThreshold;
    private float _critChance;

    private bool _isHolstered;
    public bool IsBusy => _state.isReloading || _isHolstered || _isSwapping;
    public float CurrentSpread => _state.currentSpread;
    private bool _isSwapping;

    private static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int AnimShoot = Animator.StringToHash("Shoot");
    private static readonly int AnimReload = Animator.StringToHash("Reload");
    private static readonly int AnimIsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int AnimEquip = Animator.StringToHash("Equip");
    private static readonly int AnimHolster = Animator.StringToHash("Holster");
    private static readonly int AnimIsReloading = Animator.StringToHash("IsReloading");
    private static readonly int AnimAction = Animator.StringToHash("Action");

    private void Awake()
    {
        if (_input == null) _input = GetComponentInParent<PlayerInputHub>();
        if (_look == null) _look = GetComponentInParent<PlayerLook>();
        if (_muzzleFx == null) _muzzleFx = GetComponentInChildren<ParticleSystem>(true);
        if (_muzzleLight == null) _muzzleLight = GetComponentInChildren<Light>(true);
        if (_muzzleLight != null) _muzzleLight.enabled = false;
        if (_stats == null) _stats = GetComponentInParent<PlayerStats>();

        // 업그레이드 이벤트 구독
        if (_upgradeManager != null)
            _upgradeManager.OnStatsChanged += ApplyUpgradeStats;

        if (_data != null)
            Equip(_data);
        else
            Debug.LogWarning("[GunController] WeaponData가 지정되지 않았습니다.");
    }

    private void OnDestroy()
    {
        if (_upgradeManager != null)
            _upgradeManager.OnStatsChanged -= ApplyUpgradeStats;
    }

    private void Start()
    {
        NotifyAmmo();
    }

    private void Update()
    {
        UpdateMovementAnim();
        UpdateSpreadRecover();
        HandleFire();
        HandleReload();
        HandleReloadFinish();
        HandleShotgunReloadTick();
    }

    public void Equip(WeaponData newData)
    {
        if (newData == null)
        {
            return;
        }

        _data = newData;
        ApplyWeaponData(_data);
        ApplyAnimatorOverride(_data);
        _state.ResetFromData(_data);

        // 업그레이드 스탯 덮어쓰기
        if (_upgradeManager != null && _upgradeManager.IsOwned)
        {
            _state.reserveAmmo = _upgradeManager.CurrentReserveAmmo;
            ApplyUpgradeStats();
        }

        NotifyAmmo();
    }

    public void Unequip()
    {
        _pumpLocked = false;
        CancelReload();
        _anim.SetTrigger(AnimHolster);
        _data = null;
    }

    // 업그레이드 스탯 적용
    // 업그레이드 매니저의 현재 스탯을 GunController 캐시에 반영
    // OnStatsChanged 이벤트 또는 Equip() 직후 호출
    private void ApplyUpgradeStats()
    {
        if (_upgradeManager == null || !_upgradeManager.IsOwned) return;

        _damage = _upgradeManager.CurrentDamage;
        _critChance = _upgradeManager.CurrentCritChance;

        // 업그레이드로 늘어난 차액만큼만 현재 탄창에 추가
        int newMagSize = _upgradeManager.CurrentMagSize;
        int magDiff = newMagSize - _magSize;
        if (magDiff > 0)
            _state.ammoInMag = Mathf.Min(_state.ammoInMag + magDiff, newMagSize);
        _magSize = newMagSize;

        // 업그레이드로 늘어난 차액만큼 예비 탄약 추가
        int newReserve = _upgradeManager.CurrentReserveAmmo;
        if (_state.reserveAmmo < newReserve)
            _state.reserveAmmo = newReserve;

        NotifyAmmo();
    }

    // 발사
    private void HandleFire()
    {
        if (_isHolstered) return;

        // 미구매 무기 발사 차단
        if (_upgradeManager != null && !_upgradeManager.IsOwned) return;

        bool isSprinting = _input.SprintHeld && _input.Move.sqrMagnitude > 0.01f;
        if (isSprinting) return;
        if (_state.isReloading) return;
        if (_pumpLocked) return;

        bool wantFire = (_data != null && _data.fireMode == FireMode.SemiAuto)
            ? _input.FirePressedThisFrame
            : _input.FireHeld;
        if (!wantFire) return;

        if (_state.ammoInMag <= 0) return;
        if (Time.time < _state.nextFireTime) return;

        _state.nextFireTime = Time.time + (1f / _fireRate);
        _state.ammoInMag--;
        NotifyAmmo();
        Fire();
    }

    private void Fire()
    {
        _anim.SetTrigger(AnimShoot);

        bool isShotgun = (_data != null && _data.reloadType == ReloadType.PerShell);
        if (isShotgun) _pumpLocked = true;

        if (_muzzleFx != null)
        {
            _muzzleFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _muzzleFx.Play(true);
        }

        if (_muzzleLight != null)
        {
            if (_muzzleLightCo != null) StopCoroutine(_muzzleLightCo);
            _muzzleLightCo = StartCoroutine(CoMuzzleLight());
        }

        bool isAiming = _input.AimHeld;
        float recoilMult = isAiming ? _adsRecoilMult : 1f;
        float spreadMult = isAiming ? _adsSpreadMult : 1f;

        float randomYaw = UnityEngine.Random.Range(-_recoilYawAmount, _recoilYawAmount);
        _look.AddRecoil(_recoilPitchAmount * recoilMult, randomYaw * recoilMult);

        if (Time.time - _state.lastShotTime > _tapResetTime)
            _state.currentSpread = 0f;

        _state.lastShotTime = Time.time;

        bool isAirborne = _playerMove != null && !_playerMove.IsGrounded;
        float airMult = isAirborne ? _airSpreadMultiplier : 1f;

        _state.currentSpread = Mathf.Clamp(
            _state.currentSpread + _spreadAmount * spreadMult * airMult,
            0f, _maxSpread * spreadMult * airMult);

        if (isShotgun)
            FireShotgunPellets();
        else
            FireSingleRay(_damage);
    }

    private void FireShotgunPellets()
    {
        if (_data == null) return;

        int pellets = Mathf.Max(1, _data.pelletCount);
        float spread = Mathf.Max(0f, _data.pelletSpread);

        // 샷건 업그레이드 스탯 적용
        float pelletDmg = _upgradeManager != null && _upgradeManager.IsOwned
            ? _upgradeManager.CurrentDamage
            : _data.damagePerPellet;

        pellets = _upgradeManager != null && _upgradeManager.IsOwned
            ? _upgradeManager.CurrentPelletCount
            : pellets;

        for (int i = 0; i < pellets; i++)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * spread;
            Vector3 dir = (_cam.transform.forward
                           + _cam.transform.right * r.x
                           + _cam.transform.up * r.y).normalized;

            FireRay(new Ray(_cam.transform.position, dir), pelletDmg, spawnImpactFx: true);
        }
    }

    private void FireSingleRay(float damage)
    {
        Vector3 direction = _cam.transform.forward
            + _cam.transform.right * UnityEngine.Random.Range(-_state.currentSpread, _state.currentSpread)
            + _cam.transform.up * UnityEngine.Random.Range(-_state.currentSpread, _state.currentSpread);
        direction.Normalize();

        FireRay(new Ray(_cam.transform.position, direction), damage, spawnImpactFx: true);
    }

    private void FireRay(Ray ray, float damage, bool spawnImpactFx)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit, _range)) return;

        Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

        EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            bool headshot = hit.collider.CompareTag("EnemyHead");

            // 치명타 판정
            bool isCrit = _critChance > 0f && UnityEngine.Random.value < _critChance;
            float finalDamage = isCrit ? damage * 2f : damage;

            if (isCrit)
                Debug.Log($"[GunController] 치명타! {damage:F1} → {finalDamage:F1}");

            enemy.TakeDamage((int)finalDamage, headshot, hit.point, hit.normal);
            return;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("InvisibleWall")) return;

        // Hit Spark
        if (_hitSparkPrefab != null)
        {
            GameObject spark = Instantiate(_hitSparkPrefab,
                hit.point + hit.normal * 0.02f,
                Quaternion.LookRotation(hit.normal));
            Destroy(spark, 1.5f);
        }

        // Bullet Hole
        if (_bulletHolePrefab != null)
        {
            Vector3 pos = hit.point + hit.normal * 0.01f;
            Quaternion rot = Quaternion.LookRotation(-hit.normal)
                             * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

            GameObject hole = Instantiate(_bulletHolePrefab, pos, rot);
            hole.transform.SetParent(hit.collider.transform);

            _bulletHoles.Enqueue(hole);
            while (_bulletHoles.Count > _maxBulletHoles)
            {
                GameObject old = _bulletHoles.Dequeue();
                if (old != null) Destroy(old);
            }
        }
    }

    private IEnumerator CoMuzzleLight()
    {
        _muzzleLight.enabled = true;
        yield return new WaitForSeconds(_muzzleLightDuration);
        _muzzleLight.enabled = false;
    }

    // 재장전
    private void HandleReload()
    {
        if (_pumpLocked) return;
        if (_isHolstered) return;
        if (_upgradeManager != null && !_upgradeManager.IsOwned) return;
        bool isSprinting = _input.SprintHeld && _input.Move.sqrMagnitude > 0.01f;
        if (isSprinting) return;
        if (_state.isReloading) return;
        if (!_input.ReloadPressedThisFrame) return;
        StartReload();
    }

    private void StartReload()
    {
        if (_state.isReloading) return;
        if (_data == null) return;
        if (_state.ammoInMag >= _magSize) return;
        if (!_data.infiniteReserveAmmo && _state.reserveAmmo <= 0) return;

        _state.isReloading = true;
        _state.nextFireTime = Time.time;

        if (_stats != null) _stats.SetReloading(true);
        SafeSetBool(AnimIsReloading, true);

        if (_data.reloadType == ReloadType.Magazine)
        {
            _state.reloadEndTime = Time.time + _reloadDuration;
            _anim.SetTrigger(AnimReload);
            return;
        }

        _state.reloadEndTime = Time.time + _data.reloadStartTime;
        _anim.SetTrigger(AnimReload);
    }

    private void HandleReloadFinish()
    {
        if (!_state.isReloading || _data == null) return;
        if (_data.reloadType != ReloadType.Magazine) return;
        if (Time.time < _state.reloadEndTime) return;
        FinishReload_Magazine();
    }

    private void FinishReload_Magazine()
    {
        _state.isReloading = false;
        SafeSetBool(AnimIsReloading, false);
        if (_stats != null) _stats.SetReloading(false);

        int need = _magSize - _state.ammoInMag;
        if (need <= 0) return;

        int take = _data.infiniteReserveAmmo
            ? need
            : Mathf.Min(need, _state.reserveAmmo);

        if (!_data.infiniteReserveAmmo)
            _state.reserveAmmo -= take;

        _state.ammoInMag += take;
        NotifyAmmo();
    }

    private void HandleShotgunReloadTick()
    {
        if (_data == null || _data.reloadType != ReloadType.PerShell) return;
        if (!_state.isReloading) return;

        bool wantFire = (_data.fireMode == FireMode.SemiAuto)
            ? _input.FirePressedThisFrame
            : _input.FireHeld;
        if (wantFire && _state.ammoInMag > 0) { StopShotgunReload(); return; }

        if (Time.time < _state.reloadEndTime) return;

        bool noAmmo = !_data.infiniteReserveAmmo && _state.reserveAmmo <= 0;
        if (_state.ammoInMag >= _magSize || noAmmo) { StopShotgunReload(); return; }

        if (!_data.infiniteReserveAmmo) _state.reserveAmmo--;
        _state.ammoInMag++;
        NotifyAmmo();

        _state.reloadEndTime = Time.time + _data.perShellTime;
    }

    private void StopShotgunReload()
    {
        _state.isReloading = false;
        SafeSetBool(AnimIsReloading, false);
        if (_stats != null) _stats.SetReloading(false);
    }

    private void CancelReload()
    {
        _state.isReloading = false;
        _state.reloadEndTime = 0f;
        SafeSetBool(AnimIsReloading, false);
        if (_stats != null) _stats.SetReloading(false);
    }

    // 유틸
    private void UpdateSpreadRecover()
    {
        _state.currentSpread = Mathf.Lerp(_state.currentSpread, 0f, _spreadRecoverSpeed * Time.deltaTime);
    }

    private void UpdateMovementAnim()
    {
        bool isMoving = _input.Move.sqrMagnitude > (_moveThreshold * _moveThreshold);
        bool canSprint = _stats != null && _stats.CanSprint;
        bool isSprinting = _input.SprintHeld && isMoving && canSprint && !_state.isReloading;

        _anim.SetBool(AnimIsMoving, isMoving);
        _anim.SetBool(AnimIsSprinting, isSprinting);
    }

    private void NotifyAmmo() => OnAmmoChanged?.Invoke(_state.ammoInMag, _state.reserveAmmo);
    public void ForceNotifyAmmo() => NotifyAmmo();

    private void ApplyWeaponData(WeaponData data)
    {
        _fireRate = data.fireRate;
        _range = data.range;
        _damage = data.damage;
        _magSize = data.magSize;
        _reloadDuration = data.reloadDuration;
        _recoilPitchAmount = data.recoilPitchAmount;
        _recoilYawAmount = data.recoilYawAmount;
        _spreadAmount = data.spreadAmount;
        _maxSpread = data.maxSpread;
        _spreadRecoverSpeed = data.spreadRecoverSpeed;
        _tapResetTime = data.tapResetTime;
        _moveThreshold = data.moveThreshold;
        _critChance = 0f;   // 업그레이드 적용 전 초기화
    }

    private void ApplyAnimatorOverride(WeaponData data)
    {
        if (_anim != null && data.animatorOverride != null)
            _anim.runtimeAnimatorController = data.animatorOverride;
    }

    public void BeginHolsterForSwap()
    {
        _pumpLocked = false;
        if (_isSwapping) return;

        if (_muzzleFx != null)
            _muzzleFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (_muzzleLight != null) _muzzleLight.enabled = false;
        if (_muzzleLightCo != null) { StopCoroutine(_muzzleLightCo); _muzzleLightCo = null; }

        CancelReload();
        _isHolstered = true;
        _isSwapping = true;
    }

    public void CancelSwapState() => _isSwapping = false;

    public void OnHolsterAnimEnd() { }

    public void OnEquipAnimEnd()
    {
        _isHolstered = false;
        _isSwapping = false;
        _pumpLocked = false;
    }

    public void ForceEquipData(WeaponData newData)
    {
        Equip(newData);
        _anim.ResetTrigger(AnimHolster);
        _anim.SetTrigger(AnimEquip);
    }

    public void OnPumpEnd() => _pumpLocked = false;

    private void SafeSetBool(int hash, bool value)
    {
        foreach (var p in _anim.parameters)
            if (p.nameHash == hash) { _anim.SetBool(hash, value); return; }
    }
}