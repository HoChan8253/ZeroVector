using System;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Refs")]
    public Camera _cam;
    public Animator _anim;
    public PlayerInputHub _input;

    [Header("Animation")]
    public float _moveThreshold = 0.1f;

    [Header("Gun Settings")]
    public float _fireRate = 10f;
    public float _range = 100f;
    public float _damage = 25f;

    [Header("Spread")]
    public float _spreadAmount = 0.02f;   // 기본 퍼짐
    public float _maxSpread = 0.08f;      // 최대 퍼짐
    public float _spreadRecoverSpeed = 8f;   // 퍼짐 복구 속도
    public float _tapResetTime = 0.15f;      // 이 시간 이상 쉬면 단발로 간주
    private float _lastShotTime;

    private float _currentSpread;

    private float _nextFireTime;

    [Header("Ammo")]
    public int _magSize = 30;          // 탄창 용량
    public int _ammoInMag = 30;        // 현재 탄창 탄 수
    public int _reserveAmmo = 90;      // 예비탄

    public event Action<int, int> OnAmmoChanged;

    [Header("Reload")]
    public float _reloadDuration = 4.583f;
    private bool _isReloading;
    private float _reloadEndTime;

    public PlayerLook _look;
    public float _recoilPitchAmount = 2f;
    public float _recoilYawAmount = 1f;

    private void Awake()
    {
        if (_input == null)
            _input = GetComponentInParent<PlayerInputHub>();

        if (_look == null)
            _look = GetComponentInParent<PlayerLook>();

        _lastShotTime = -999f; // 첫 발사는 항상 리셋
    }

    private void Update()
    {
        // 연사 조건
        if (!_isReloading && _ammoInMag > 0 && _input.FireHeld && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + (1f / _fireRate);

            _ammoInMag--;
            NotifyAmmo();

            Fire();
        }

        if (!_isReloading && _ammoInMag <= 0 && _input.FireHeld)
        {
            // _anim.SetTrigger("NoAmmo");
        }

        if (_input.ReloadPressedThisFrame && !_isReloading)
        {
            StartReload();
        }

        if (_isReloading && Time.time >= _reloadEndTime)
        {
            //_isReloading = false;
            FinishReload();
        }

        _currentSpread = Mathf.Lerp(_currentSpread, 0f, _spreadRecoverSpeed * Time.deltaTime);

        bool isMoving = _input.Move.sqrMagnitude > (_moveThreshold * _moveThreshold);
        _anim.SetBool("IsMoving", isMoving);
    }

    private void Fire()
    {
        _anim.SetTrigger("Shoot");

        float randomYaw = UnityEngine.Random.Range(-_recoilYawAmount, _recoilYawAmount);
        _look.AddRecoil(_recoilPitchAmount, randomYaw);

        // 단발 리셋
        if (Time.time - _lastShotTime > _tapResetTime)
        {
            _currentSpread = 0f;
        }

        _lastShotTime = Time.time;

        // 퍼짐 증가
        _currentSpread += _spreadAmount;
        _currentSpread = Mathf.Clamp(_currentSpread, 0f, _maxSpread);

        Vector3 direction = _cam.transform.forward;

        direction += _cam.transform.right * UnityEngine.Random.Range(-_currentSpread, _currentSpread);
        direction += _cam.transform.up * UnityEngine.Random.Range(-_currentSpread, _currentSpread);
        direction.Normalize();

        Ray ray = new Ray(_cam.transform.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, _range))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
                h.TakeDamage(_damage);
        }
    }

    private void StartReload()
    {
        if (_isReloading) return;
        if (_ammoInMag >= _magSize) return;
        if (_reserveAmmo <= 0) return;

        _isReloading = true;
        _reloadEndTime = Time.time + _reloadDuration;
        _anim.SetTrigger("Reload");
    }

    private void FinishReload()
    {
        _isReloading = false;

        int need = _magSize - _ammoInMag;
        if (need <= 0) return;

        int take = Mathf.Min(need, _reserveAmmo);
        _reserveAmmo -= take;
        _ammoInMag += take;

        NotifyAmmo();
    }

    private void NotifyAmmo()
    {
        OnAmmoChanged?.Invoke(_ammoInMag, _reserveAmmo);
    }

    private void Start()
    {
        NotifyAmmo(); // 시작 시 UI 갱신
    }
}