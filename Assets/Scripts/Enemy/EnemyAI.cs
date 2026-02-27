using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private enum State { DayIdle, DayPatrol, Chase, Attack, Stun, Dead }

    [Header("Refs")]
    [SerializeField] private Transform _player;
    [SerializeField] private Animator _anim;
    [SerializeField] private NavMeshAgent _agent;

    [Header("Ranges")]
    [SerializeField] private float _aggroRange = 10f;
    [SerializeField] private float _deaggroRange = 18f;
    [SerializeField] private float _attackRange = 2.0f;
    [SerializeField] private float _attackCooldown = 1.2f;

    [Header("Day Loop")]
    [SerializeField] private float _dayIdleTime = 2.0f;
    [SerializeField] private float _dayWalkTime = 3.0f;
    [SerializeField] private float _patrolRadius = 6f;

    [Header("Stun")]
    [SerializeField] private float _stunTime = 1.0f;

    private State _state;
    private float _stateEndTime;
    private float _nextAttackTime;
    private bool _aggro;

    private void Awake()
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        EnterDayIdle();
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
        FaceTarget(_player.position);

        if (d > _attackRange * 1.1f)
        {
            EnterChase();
            return;
        }

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + _attackCooldown;
            _anim.SetTrigger("Attack");
        }
    }

    private void EnterDayIdle()
    {
        _state = State.DayIdle;
        _stateEndTime = Time.time + _dayIdleTime;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    private void EnterDayPatrol()
    {
        _state = State.DayPatrol;
        _stateEndTime = Time.time + _dayWalkTime;
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
        _anim.SetTrigger("Stun");
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

        _anim.SetFloat("Speed", normalized);
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

        if (stun) EnterStun();
        else if (_state == State.DayIdle || _state == State.DayPatrol) EnterChase();
    }

    public void Die()
    {
        if (_state == State.Dead) return;

        _state = State.Dead;

        _agent.isStopped = true;
        _agent.ResetPath();

        _anim.SetBool("Dead", true);

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
}