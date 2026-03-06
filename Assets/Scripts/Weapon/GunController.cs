using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunController : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData _data;

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

    // 런타임 상태
    [SerializeField] private WeaponState _state = new WeaponState();

    // 캐싱
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

    private bool _isHolstered;

    public bool IsBusy => _state.isReloading || _isHolstered || _isSwapping;
    private bool _isSwapping;

    // 애니메이터 파라미터 (규격 고정)
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
        if (_input == null)
            _input = GetComponentInParent<PlayerInputHub>();

        if (_look == null)
            _look = GetComponentInParent<PlayerLook>();

        if (_muzzleFx == null)
            _muzzleFx = GetComponentInChildren<ParticleSystem>(true);

        if (_muzzleLight == null)
            _muzzleLight = GetComponentInChildren<Light>(true);

        if (_muzzleLight != null)
            _muzzleLight.enabled = false;

        if (_stats == null)
            _stats = GetComponentInParent<PlayerStats>();

        // 최초 장착
        if (_data != null)
            Equip(_data);
        else
            Debug.LogWarning("[GunController] WeaponData가 지정되지 않았습니다.");
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
            Debug.LogWarning("[GunController] Equip 실패: newData == null");
            return;
        }

        _data = newData;

        ApplyWeaponData(_data);
        ApplyAnimatorOverride(_data);
        _state.ResetFromData(_data);

        // 시작 상태 UI 반영
        NotifyAmmo();
    }

    public void Unequip()
    {
        // 여기서 트리거/상태 초기화
        _pumpLocked = false;
        CancelReload();
        _anim.SetTrigger(AnimHolster);
        _data = null;
    }

    private void HandleFire()
    {
        if (_isHolstered) return;
        bool isSprinting = _input.SprintHeld && _input.Move.sqrMagnitude > 0.01f;
        if (isSprinting) return;

        if (_state.isReloading) return;
        if (_pumpLocked) return;

        bool wantFire = (_data != null && _data.fireMode == FireMode.SemiAuto)
        ? _input.FirePressedThisFrame
        : _input.FireHeld;
        if (!wantFire) return;

        if (_state.ammoInMag <= 0)
        {
            // 여기서 DryFire 트리거 추가
            return;
        }

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

        if (isShotgun)
            _pumpLocked = true;

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

        float randomYaw = UnityEngine.Random.Range(-_recoilYawAmount, _recoilYawAmount);
        _look.AddRecoil(_recoilPitchAmount, randomYaw);

        if (Time.time - _state.lastShotTime > _tapResetTime)
            _state.currentSpread = 0f;

        _state.lastShotTime = Time.time;

        _state.currentSpread += _spreadAmount;
        _state.currentSpread = Mathf.Clamp(_state.currentSpread, 0f, _maxSpread);

        // 분기점
        if (isShotgun)
            FireShotgunPellets();
        else
            FireSingleRay(_damage);
    }

    private void FireShotgunPellets()
    {
        if (_data == null) return;

        // 6~8발 랜덤
        int pellets = Mathf.Max(1, _data.pelletCount);

        float spread = Mathf.Max(0f, _data.pelletSpread);

        for (int i = 0; i < pellets; i++)
        {
            Vector2 r = UnityEngine.Random.insideUnitCircle * spread;

            Vector3 dir = _cam.transform.forward
                          + _cam.transform.right * r.x
                          + _cam.transform.up * r.y;

            dir.Normalize();

            Ray ray = new Ray(_cam.transform.position, dir);

            // 펠릿당 데미지
            FireRay(ray, _data.damagePerPellet, spawnImpactFx: true);
        }
    }

    private void FireSingleRay(float damage)
    {
        Vector3 direction = _cam.transform.forward;
        direction += _cam.transform.right * UnityEngine.Random.Range(-_state.currentSpread, _state.currentSpread);
        direction += _cam.transform.up * UnityEngine.Random.Range(-_state.currentSpread, _state.currentSpread);
        direction.Normalize();

        Ray ray = new Ray(_cam.transform.position, direction);
        FireRay(ray, damage, spawnImpactFx: true);
    }

    private void FireRay(Ray ray, float damage, bool spawnImpactFx)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, _range))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                bool headshot = hit.collider.CompareTag("EnemyHead");

                Debug.Log($"Pellet Hit: {damage} (headshot={headshot})");

                enemy.TakeDamage((int)damage, headshot, hit.point, hit.normal);
                return;
            }

            // Bullet Spark
            if (_hitSparkPrefab != null)
            {
                Vector3 sparkPos = hit.point + hit.normal * 0.02f;
                Quaternion rot = Quaternion.LookRotation(hit.normal);

                GameObject spark = Instantiate(_hitSparkPrefab, sparkPos, rot);
                Destroy(spark, 1.5f);
            }

            // Bullet Hole
            if (_bulletHolePrefab != null)
            {
                Vector3 pos = hit.point + hit.normal * 0.01f;
                Quaternion rot = Quaternion.LookRotation(-hit.normal);
                rot *= Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

                GameObject hole = Instantiate(_bulletHolePrefab, pos, rot);
                hole.transform.SetParent(hit.collider.transform);

                _bulletHoles.Enqueue(hole);

                while (_bulletHoles.Count > _maxBulletHoles)
                {
                    GameObject old = _bulletHoles.Dequeue();
                    if (old == null) continue;
                    Destroy(old);
                }
            }
        }
    }

    private IEnumerator CoMuzzleLight()
    {
        _muzzleLight.enabled = true;
        yield return new WaitForSeconds(_muzzleLightDuration);
        _muzzleLight.enabled = false;
    }

    private void HandleReload()
    {
        if (_pumpLocked) return;

        if (_isHolstered) return;
        bool isSprinting = _input.SprintHeld && _input.Move.sqrMagnitude > 0.01f;
        if (isSprinting) return;    // Sprint 중 재장전 불가

        if (_state.isReloading) return;
        if (!_input.ReloadPressedThisFrame) return;

        StartReload();
    }

    private void StartReload()
    {
        if (_state.isReloading) return;

        if (_data == null) return;
        if (_state.ammoInMag >= _magSize) return;
        if (_state.reserveAmmo <= 0) return;

        _state.isReloading = true;
        _state.nextFireTime = Time.time;

        if (_stats != null)
            _stats.SetReloading(true);

        _anim.SetBool(AnimIsReloading, true);

        if (_data.reloadType == ReloadType.Magazine)
        {
            _state.reloadEndTime = Time.time + _reloadDuration;
            _anim.SetTrigger(AnimReload);
            return;
        }

        // Shotgun
        _state.reloadEndTime = Time.time + _data.reloadStartTime; // IdleToReload 지나고 첫 장전
        _anim.SetTrigger(AnimReload); // AnyState -> IdleToReload
    }

    private void HandleReloadFinish()
    {
        if (!_state.isReloading) return;
        if (_data == null) return;

        if (_data.reloadType != ReloadType.Magazine) return;

        if (Time.time < _state.reloadEndTime) return;
        FinishReload_Magazine();
    }

    private void FinishReload_Magazine()
    {
        _state.isReloading = false;
        _anim.SetBool(AnimIsReloading, false);

        if (_stats != null)
            _stats.SetReloading(false);

        int need = _magSize - _state.ammoInMag;
        if (need <= 0) return;

        int take = Mathf.Min(need, _state.reserveAmmo);
        _state.reserveAmmo -= take;
        _state.ammoInMag += take;
        NotifyAmmo();
    }

    private void HandleShotgunReloadTick()
    {
        if (_data == null) return;
        if (_data.reloadType != ReloadType.PerShell) return;
        if (!_state.isReloading) return;

        // 장전 중 사격 입력이면 끊기
        bool wantFire = (_data.fireMode == FireMode.SemiAuto) ? _input.FirePressedThisFrame : _input.FireHeld;
        if (wantFire && _state.ammoInMag > 0)
        {
            StopShotgunReload();
            return;
        }

        if (Time.time < _state.reloadEndTime) return;

        // 종료 조건(꽉 참 / 탄 없음)
        if (_state.ammoInMag >= _magSize || _state.reserveAmmo <= 0)
        {
            StopShotgunReload();
            return;
        }

        // 쉘 1발 장전
        _state.reserveAmmo--;
        _state.ammoInMag++;
        NotifyAmmo();

        // 다음 쉘 타이밍 예약
        _state.reloadEndTime = Time.time + _data.perShellTime;
    }

    private void StopShotgunReload()
    {
        _state.isReloading = false;
        _anim.SetBool(AnimIsReloading, false);

        if (_stats != null)
            _stats.SetReloading(false);
    }

    private void CancelReload()
    {
        _state.isReloading = false;
        _state.reloadEndTime = 0f;
        _anim.SetBool(AnimIsReloading, false);

        if (_stats != null)
            _stats.SetReloading(false);
    }

    private void UpdateSpreadRecover()
    {
        _state.currentSpread = Mathf.Lerp(_state.currentSpread, 0f, _spreadRecoverSpeed * Time.deltaTime);
    }

    private void UpdateMovementAnim()
    {
        bool isMoving = _input.Move.sqrMagnitude > (_moveThreshold * _moveThreshold);

        bool canSprint = (_stats != null) && _stats.CanSprint;

        bool isSprinting = _input.SprintHeld && isMoving && canSprint && !_state.isReloading;

        _anim.SetBool(AnimIsMoving, isMoving);
        _anim.SetBool(AnimIsSprinting, isSprinting);
    }

    private void NotifyAmmo()
    {
        OnAmmoChanged?.Invoke(_state.ammoInMag, _state.reserveAmmo);
    }

    public void ForceNotifyAmmo()
    {
        NotifyAmmo();
    }

    private void ApplyWeaponData(WeaponData data)
    {
        // 세팅
        _fireRate = data.fireRate;
        _range = data.range;
        _damage = data.damage;

        // 장탄수
        _magSize = data.magSize;

        // 재장전
        _reloadDuration = data.reloadDuration;

        // 반동
        _recoilPitchAmount = data.recoilPitchAmount;
        _recoilYawAmount = data.recoilYawAmount;

        // 탄퍼짐
        _spreadAmount = data.spreadAmount;
        _maxSpread = data.maxSpread;
        _spreadRecoverSpeed = data.spreadRecoverSpeed;
        _tapResetTime = data.tapResetTime;

        // 애니메이션
        _moveThreshold = data.moveThreshold;
    }

    private void ApplyAnimatorOverride(WeaponData data)
    {
        if (_anim == null) return;

        if (data.animatorOverride != null)
        {
            _anim.runtimeAnimatorController = data.animatorOverride;
        }
    }

    public void BeginHolsterForSwap()
    {
        _pumpLocked = false;

        if (_isSwapping) return;

        if (_muzzleFx != null)
            _muzzleFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_muzzleLight != null)
            _muzzleLight.enabled = false;

        if (_muzzleLightCo != null)
        {
            StopCoroutine(_muzzleLightCo);
            _muzzleLightCo = null;
        }

        // 스왑 중에는 발사/장전 막기
        CancelReload();
        _isHolstered = true;
        _isSwapping = true;
    }

    public void CancelSwapState()
    {
        _isSwapping = false;
    }

    // Holster 애니 끝 프레임에 Animation Event로 호출
    public void OnHolsterAnimEnd()
    {
        // 여기서는 매니저가 실제 교체를 수행
    }

    // Equip 애니 끝 프레임에 Animation Event로 호출
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

    public void OnPumpEnd()
    {
        _pumpLocked = false;
    }
}