using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private enum State { DayIdle, DayPatrol, Chase, Attack, Stun, Dead }

    [Header("Refs")]
    [SerializeField] private Transform _player;
    [SerializeField] private EnemyAnimation _animCtrl;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private EnemyData _data;

    public Transform Target => _player;
    public EnemyData Data => _data;

    [Header("Ranged Refs")]
    [SerializeField] private GameObject _energyBallPrefab;
    [SerializeField] private Transform _bulletSpawner;

    [Header("VFX")]
    [SerializeField] private ParticleSystem _muzzleFxPrefab;
    private ParticleSystem _muzzleFxInstance;

    public bool IsMoving =>
    _state == State.Chase || _state == State.DayPatrol;

    public bool IsAttacking =>
        _state == State.Attack;

    public bool IsDead => _state == State.Dead;

    // Default Values (ScriptableObject 없이도 동작 가능)
    private float _aggroRange = 10f;
    private float _deaggroRange = 18f;
    private float _damageAggroHoldTime = 2.0f;

    private float _attackRange = 2.0f;
    private float _attackCooldown = 1.2f;

    private float _projectileSpeed = 12f;
    private float _projectileLifeTime = 2.0f;
    private int _projectileDamage = 10;

    private float _dayIdleTime = 2.0f;
    private float _dayWalkTime = 3.0f;
    private float _patrolRadius = 6f;

    private float _stunTime = 1.0f;

    // Runtime
    private float _forcedAggroUntil;
    private State _state;
    private float _stateEndTime;
    private float _nextAttackTime;
    private bool _aggro;

    private void Awake()
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_animCtrl == null) _animCtrl = GetComponentInChildren<EnemyAnimation>();
        if (_animCtrl == null) _animCtrl = GetComponentInChildren<EnemyAnimation>(true);

        ApplyData();

        if (_muzzleFxPrefab != null && _bulletSpawner != null)
        {
            _muzzleFxInstance = Instantiate(_muzzleFxPrefab, _bulletSpawner);
            _muzzleFxInstance.transform.localPosition = Vector3.zero;
            _muzzleFxInstance.transform.localRotation = Quaternion.identity;
            _muzzleFxInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Start()
    {
        StartCoroutine(CoRandomStart());
    }

    private void Update()
    {
        if (_state == State.Dead) return;

        if (_player != null)
        {
            float dist = DistanceToPlayer();

            if (DayNightManager.Instance.IsNight)
            {
                _aggro = true;
            }
            else
            {
                if (Time.time < _forcedAggroUntil)
                {
                    _aggro = true;
                }
                else
                {
                    if (!_aggro)
                    {
                        if (dist <= _aggroRange) _aggro = true;
                    }
                    else
                    {
                        if (dist >= _deaggroRange)
                        {
                            _aggro = false;

                            if (_state == State.Chase || _state == State.Attack)
                                EnterDayIdle();
                        }
                    }
                }
            }
        }

        switch (_state)
        {
            case State.DayIdle:
            case State.DayPatrol:
                if (_aggro) EnterChase();
                else UpdateDayLoop();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Stun:
                if (Time.time >= _stateEndTime) EnterChaseOrDay();
                break;
        }

        UpdateAnimatorSpeed();
    }

    private IEnumerator CoRandomStart()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));
        EnterDayIdle();
    }

    private void ApplyData()
    {
        if (_data == null) return;

        _aggroRange = _data.aggroRange;
        _deaggroRange = _data.deaggroRange;
        _damageAggroHoldTime = _data.damageAggroHoldTime;

        _attackRange = _data.attackRange;
        _attackCooldown = _data.attackCooldown;

        _dayIdleTime = _data.dayIdleTime;
        _dayWalkTime = _data.dayWalkTime;
        _patrolRadius = _data.patrolRadius;

        _stunTime = _data.stunTime;

        _projectileSpeed = _data.projectileSpeed;
        _projectileLifeTime = _data.projectileLifeTime;
        _projectileDamage = _data.projectileDamage;
    }

    private void UpdateDayLoop()
    {
        if (Time.time < _stateEndTime) return;

        if (_state == State.DayIdle) EnterDayPatrol();
        else EnterDayIdle();
    }

    private void UpdateChase()
    {
        if (_player == null) { EnterDayIdle(); return; }

        float d = DistanceToPlayer();

        _agent.isStopped = false;
        _agent.SetDestination(_player.position);

        if (d <= _attackRange) EnterAttack();
    }

    private void UpdateAttack()
    {
        if (_player == null) { EnterDayIdle(); return; }

        float d = DistanceToPlayer();

        bool isMelee = (_data == null) || (_data.attackType == EnemyAttackType.Melee);

        if (isMelee)
            FaceTarget(_player.position);

        if (d > _attackRange * 1.1f)
        {
            EnterChase();
            return;
        }

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + _attackCooldown;
            _animCtrl?.PlayAttack();

            if (_data != null && _data.attackType == EnemyAttackType.Ranged)
            {
                PlayMuzzleFx();
            }
            
            FireEnergyBallTriple();
        }
    }

    private void EnterDayIdle()
    {
        _state = State.DayIdle;

        float randomIdle = Random.Range(_dayIdleTime * 0.7f, _dayIdleTime * 1.3f);
        _stateEndTime = Time.time + randomIdle;

        _agent.isStopped = true;
        _agent.ResetPath();
    }

    private void EnterDayPatrol()
    {
        _state = State.DayPatrol;

        float randomWalk = Random.Range(_dayWalkTime * 0.7f, _dayWalkTime * 1.3f);
        _stateEndTime = Time.time + randomWalk;

        _agent.isStopped = false;

        if (TryGetRandomPoint(transform.position, _patrolRadius, out var p))
            _agent.SetDestination(p);
        else
            _agent.ResetPath();
    }

    private void EnterChase()
    {
        _state = State.Chase;
        _agent.isStopped = false;
    }

    private void EnterAttack()
    {
        _state = State.Attack;
        _agent.isStopped = true;
        _agent.ResetPath();
        _nextAttackTime = Mathf.Max(_nextAttackTime, Time.time);
    }

    private void EnterStun()
    {
        _state = State.Stun;
        _stateEndTime = Time.time + _stunTime;
        _agent.isStopped = true;
        _agent.ResetPath();
        _animCtrl?.PlayStun();
    }

    private void EnterChaseOrDay()
    {
        if (DayNightManager.Instance.IsNight || _aggro) EnterChase();
        else EnterDayIdle();
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _player.position);
    }

    private void FaceTarget(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }

    private void UpdateAnimatorSpeed()
    {
        float normalized = 0f;

        if (!_agent.isStopped && _agent.speed > 0.01f)
            normalized = Mathf.Clamp01(_agent.velocity.magnitude / _agent.speed);

        _animCtrl?.SetMoveSpeed(normalized);
    }

    private bool TryGetRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 rand = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(rand, out var hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

    // 외부 호출
    public void OnDamaged(Vector3 hitPoint, bool stun)
    {
        if (_state == State.Dead) return;

        _aggro = true;
        _forcedAggroUntil = Time.time + _damageAggroHoldTime;

        if (stun) EnterStun();
        else if (_state == State.DayIdle || _state == State.DayPatrol) EnterChase();
    }

    public void Die()
    {
        if (_state == State.Dead) return;

        _state = State.Dead;

        _agent.isStopped = true;
        _agent.ResetPath();

        _animCtrl?.SetDead(true);

        DisableColliders();

        StartCoroutine(CoDeathRoutine());
    }

    private void DisableColliders()
    {
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols)
            c.enabled = false;
    }

    private IEnumerator CoDeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        float fadeTime = 1.5f;
        float t = 0f;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            r.material = new Material(r.material);
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

            foreach (var r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }

    // 단발 사격
    private void FireEnergyBallSingle()
    {
        if (_player == null) return;
        if (_energyBallPrefab == null || _bulletSpawner == null) return;

        Vector3 dir = _player.position - _bulletSpawner.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        SpawnEnergyBall(_bulletSpawner, dir);
    }

    // 3갈래 사격
    private void FireEnergyBallTriple()
    {
        if (_player == null) return;
        if (_energyBallPrefab == null || _bulletSpawner == null) return;

        Vector3 baseDir = _player.position - _bulletSpawner.position;
        baseDir.y = 0f;
        if (baseDir.sqrMagnitude < 0.0001f) baseDir = transform.forward;
        baseDir.Normalize();

        float spread = 12f;
        Vector3 leftDir = Quaternion.Euler(0f, -spread, 0f) * baseDir;
        Vector3 rightDir = Quaternion.Euler(0f, spread, 0f) * baseDir;

        SpawnEnergyBall(_bulletSpawner, leftDir);
        SpawnEnergyBall(_bulletSpawner, baseDir);
        SpawnEnergyBall(_bulletSpawner, rightDir);
    }

    // 투사체 발사 (ObjectPooling)
    private void SpawnEnergyBall(Transform spawner, Vector3 dir)
    {
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject obj =
            (ObjectPoolManager.Instance != null)
            ? ObjectPoolManager.Instance.Spawn(_energyBallPrefab, spawner.position, rot)
            : Instantiate(_energyBallPrefab, spawner.position, rot);

        var proj = obj.GetComponent<EnergyBall>();
        if (proj != null)
            proj.Init(_energyBallPrefab, transform, dir, _projectileSpeed, _projectileDamage, _projectileLifeTime);
    }

    private void PlayMuzzleFx()
    {
        var fx = _muzzleFxInstance != null ? _muzzleFxInstance : null;
        if (fx == null) return;

        fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fx.Play(true);
    }
}