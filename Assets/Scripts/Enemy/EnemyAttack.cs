using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("Ranged")]
    [SerializeField] private Transform _bulletSpawner;
    [SerializeField] private ParticleSystem _muzzleFxPrefab;

    [Header("AoE Prefabs")]
    [SerializeField] private AoEIndicator _aoeIndicatorPrefab;
    [SerializeField] private StraightFxProjectile _aoeLaunchFxPrefab;
    [SerializeField] private DropProjectile _aoeDropPrefab;
    [SerializeField] private LayerMask _groundMask = ~0;

    [Header("Impact VFX")]
    [SerializeField] private GameObject _impactFxPrefab;
    [SerializeField] private float _impactFxLife = 2f;

    [Header("Melee Hit")]
    [SerializeField] private LayerMask _playerMask = ~0;

    // Runtime
    private EnemyData _data;
    private Transform _target;
    private ParticleSystem _muzzleFxInstance;
    private readonly Collider[] _meleeHits = new Collider[8];

    // Init
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

    // 공격 진입점
    public void Execute()
    {
        var type = _data != null ? _data.attackType : EnemyAttackType.Melee;

        switch (type)
        {
            case EnemyAttackType.Melee:
                // 실제 타격은 애니메이션 이벤트(AE_MeleeHit)에서 처리
                break;

            case EnemyAttackType.Ranged:
                SFXManager.PlaySound(_data.attackSoundType);
                PlayMuzzleFx();
                FireEnergyBallTriple();
                break;

            case EnemyAttackType.RangedAoe:
                SFXManager.PlaySound(_data.attackSoundType);
                StartCoroutine(CoAoeTwoPhase());
                break;
        }
    }

    // 애니메이션 이벤트
    public void AE_MeleeHit()
    {
        if (_data != null && _data.attackType != EnemyAttackType.Melee) return;
        TryMeleeHit();
    }

    // 근접
    private void TryMeleeHit()
    {
        int damage = _data != null ? _data.attackDamage : 10;
        if (damage <= 0) return;

        float radius = _data != null ? _data.meleeHitRadius : 1f;
        float forward = _data != null ? _data.meleeHitForward : 0.9f;
        float height = _data != null ? _data.meleeHitHeight : 1f;

        Vector3 center = transform.position
                         + Vector3.up * height
                         + transform.forward * forward;

        int count = Physics.OverlapSphereNonAlloc(
            center, radius, _meleeHits, _playerMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            var col = _meleeHits[i];
            if (col == null) continue;

            // 자기 자신 콜라이더 제외
            if (col.transform.IsChildOf(transform) || col.transform == transform) continue;
            if (!col.CompareTag("Player")) continue;

            var d = col.GetComponentInParent<IDamageable>();
            if (d != null) { d.TakeDamage(damage); break; }
        }
    }

    // 원거리
    private void FireEnergyBallTriple()
    {
        if (_target == null || _bulletSpawner == null) return;
        if (ObjectPoolManager.Instance == null) return;

        Vector3 baseDir = _target.position - _bulletSpawner.position;
        if (baseDir.sqrMagnitude < 0.0001f) baseDir = transform.forward;
        baseDir.Normalize();

        SpawnEnergyBall(Quaternion.Euler(0f, -12f, 0f) * baseDir);
        SpawnEnergyBall(baseDir);
        SpawnEnergyBall(Quaternion.Euler(0f, 12f, 0f) * baseDir);
    }

    private void SpawnEnergyBall(Vector3 dir)
    {
        int damage = _data != null ? _data.attackDamage : 10;
        float speed = _data != null ? _data.projectileSpeed : 12f;
        float life = _data != null ? _data.projectileLifeTime : 2f;

        var obj = ObjectPoolManager.Instance.Spawn(
            PoolKey.EnergyBall,
            _bulletSpawner.position,
            Quaternion.LookRotation(dir, Vector3.up));

        if (obj == null) return;

        var proj = obj.GetComponent<EnergyBall>();
        proj?.Init(PoolKey.EnergyBall, transform, dir, speed, damage, life);
    }

    // AoE
    private IEnumerator CoAoeTwoPhase()
    {
        if (_target == null || _bulletSpawner == null) yield break;

        // 착탄 지점 산출
        Vector3 targetPos = _target.position + Vector3.up * 10f;
        if (Physics.Raycast(targetPos, Vector3.down, out var hit, 50f, _groundMask))
            targetPos = hit.point;
        else
            targetPos = _target.position;

        // 발사 FX
        if (_aoeLaunchFxPrefab != null)
        {
            var fx = Instantiate(_aoeLaunchFxPrefab,
                _bulletSpawner.position, Quaternion.LookRotation(Vector3.up));
            fx.Init(Vector3.up, 15f, 2f);
        }

        // 경고 인디케이터
        float aoeRadius = _data != null ? _data.aoeRadius : 2.5f;
        float warnTime = _data != null ? _data.aoeWarnTime : 3f;
        float dropHeight = _data != null ? _data.aoeDropHeight : 15f;
        float dropTime = _data != null ? _data.aoeDropTime : 2f;

        var indicator = Instantiate(_aoeIndicatorPrefab);
        indicator.SetRadius(aoeRadius);
        indicator.SetPosition(targetPos);
        indicator.gameObject.SetActive(true);

        yield return new WaitForSeconds(warnTime);

        // 낙하 투사체
        Vector3 start = targetPos + Vector3.up * dropHeight;
        var drop = Instantiate(_aoeDropPrefab, start, Quaternion.identity);
        drop.Init(start, targetPos, dropTime, () =>
        {
            if (indicator != null) Destroy(indicator.gameObject);
            SpawnImpactFx(targetPos);
            DealImpactDamage(targetPos, aoeRadius);
        });
    }

    private void DealImpactDamage(Vector3 center, float radius)
    {
        int damage = _data != null ? _data.attackDamage : 10;
        if (damage <= 0) return;

        foreach (var h in Physics.OverlapSphere(center, radius, _playerMask))
        {
            if (!h.CompareTag("Player")) continue;

            var d = h.GetComponentInParent<IDamageable>();
            if (d != null) d.TakeDamage(damage);
        }
    }

    private void SpawnImpactFx(Vector3 pos)
    {
        if (_impactFxPrefab == null) return;
        Destroy(Instantiate(_impactFxPrefab, pos, Quaternion.identity), _impactFxLife);
    }

    // Muzzle FX
    private void PlayMuzzleFx()
    {
        if (_muzzleFxInstance == null) return;
        _muzzleFxInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _muzzleFxInstance.Play(true);
    }
}