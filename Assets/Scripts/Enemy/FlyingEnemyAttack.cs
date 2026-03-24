using UnityEngine;
using System.Collections;

// 공중 유닛 전용 공격 컴포넌트.
public class FlyingEnemyAttack : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private Transform _bulletSpawner;

    [Header("Burst Settings")]
    [Tooltip("한 번 공격 시 발사 횟수")]
    [SerializeField] private int _burstCount = 3;
    [Tooltip("연사 간격")]
    [SerializeField] private float _burstInterval = 0.18f;

    [Header("Visual")]
    [SerializeField] private ParticleSystem _muzzleFxPrefab;
    [SerializeField] private GameObject _impactFxPrefab;

    // Runtime
    private EnemyData _data;
    private Transform _target;
    private ParticleSystem _muzzleFxInstance;
    private bool _isBursting;

    public void Init(EnemyData data, Transform target)
    {
        _data = data;
        _target = target;

        if (_muzzleFxPrefab != null && _bulletSpawner != null)
        {
            _muzzleFxInstance = Instantiate(_muzzleFxPrefab, _bulletSpawner);
            _muzzleFxInstance.transform.localPosition = Vector3.zero;
            _muzzleFxInstance.transform.localRotation = Quaternion.identity;
            _muzzleFxInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void UpdateTarget(Transform target) => _target = target;

    // 공격 트리거 호출 시
    public void Execute(Transform target = null)
    {
        if (target != null) _target = target;
        if (_isBursting) return;  // 이전 연사가 끝나지 않았으면 무시

        StartCoroutine(CoBurst());
    }

    private IEnumerator CoBurst()
    {
        _isBursting = true;

        for (int i = 0; i < _burstCount; i++)
        {
            FireSingle();
            yield return new WaitForSeconds(_burstInterval);
        }

        _isBursting = false;
    }

    private void FireSingle()
    {
        if (_bulletSpawner == null) return;
        if (ObjectPoolManager.Instance == null) return;

        // 발사 시점의 플레이어 현재 위치를 조준
        Vector3 targetPos = _target != null
            ? _target.position + Vector3.up * 0.1f   // 살짝 위를 겨냥 (머리 높이)
            : transform.position + transform.forward * 10f;

        Vector3 dir = (targetPos - _bulletSpawner.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;

        int damage = _data != null ? _data.attackDamage : 10;
        float speed = _data != null ? _data.projectileSpeed : 15f;
        float life = _data != null ? _data.projectileLifeTime : 3f;

        var obj = ObjectPoolManager.Instance.Spawn(
            PoolKey.EnergyBall,
            _bulletSpawner.position,
            Quaternion.LookRotation(dir, Vector3.up));

        if (obj == null) return;

        var proj = obj.GetComponent<EnergyBall>();
        proj?.Init(PoolKey.EnergyBall, transform, dir, speed, damage, life);

        PlayMuzzleFx();
    }

    private void PlayMuzzleFx()
    {
        if (_muzzleFxInstance == null) return;
        _muzzleFxInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _muzzleFxInstance.Play(true);
    }
}