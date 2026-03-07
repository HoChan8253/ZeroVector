using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private enum State { DayIdle, DayPatrol, Chase, Attack, Stun, Dead }

    [Header("Refs")]
    [SerializeField] private Transform _player;
    [SerializeField] private EnemyAnimation _animCtrl;
    [SerializeField] private EnemyData _data;

    [Header("Components")]
    [SerializeField] private EnemyMovement _movement;
    [SerializeField] private EnemyAttack _attack;

    public Transform Target => _player;
    public EnemyData Data => _data;

    public bool IsMoving => _state == State.Chase || _state == State.DayPatrol;
    public bool IsAttacking => _state == State.Attack;
    public bool IsDead => _state == State.Dead;

    // 데이터 캐시
    private float AggroRange => _data != null ? _data.aggroRange : 10f;
    private float DeaggroRange => _data != null ? _data.deaggroRange : 18f;
    private float DamageAggroHold => _data != null ? _data.damageAggroHoldTime : 2f;
    private float AttackRange => _data != null ? _data.attackRange : 2f;
    private float AttackCooldown => _data != null ? _data.attackCooldown : 1.2f;
    private float DayIdleTime => _data != null ? _data.dayIdleTime : 2f;
    private float DayWalkTime => _data != null ? _data.dayWalkTime : 3f;
    private float PatrolRadius => _data != null ? _data.patrolRadius : 6f;
    private float StunTime => _data != null ? _data.stunTime : 1f;

    // Runtime
    private State _state;
    private float _stateEndTime;
    private float _nextAttackTime;
    private float _forcedAggroUntil;
    private bool _aggro;

    private void Awake()
    {
        if (_movement == null) _movement = GetComponent<EnemyMovement>();
        if (_attack == null) _attack = GetComponent<EnemyAttack>();
        if (_animCtrl == null) _animCtrl = GetComponentInChildren<EnemyAnimation>(true);

        _movement.Init(_data);
        _attack.Init(_data, _player);
    }

    private void Start()
    {
        StartCoroutine(CoRandomStart());
    }

    private void OnEnable()
    {
        if (_player == null)
            StartCoroutine(CoBindPlayer());
    }

    private void Update()
    {
        if (_state == State.Dead) return;

        UpdateAggro();
        UpdateState();
        _animCtrl?.SetMoveSpeed(_movement.GetNormalizedSpeed());
    }

    // Aggro
    private void UpdateAggro()
    {
        if (_player == null) return;

        if (DayNightManager.Instance.IsNight)
        {
            _aggro = true;
            return;
        }

        if (Time.time < _forcedAggroUntil)
        {
            _aggro = true;
            return;
        }

        float dist = DistanceToPlayer();

        if (!_aggro)
        {
            if (dist <= AggroRange) _aggro = true;
        }
        else
        {
            if (dist >= DeaggroRange)
            {
                _aggro = false;
                if (_state == State.Chase || _state == State.Attack)
                    EnterDayIdle();
            }
        }
    }

    // State Loop
    private void UpdateState()
    {
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

        _movement.ChaseTo(_player.position);

        if (DistanceToPlayer() <= AttackRange) EnterAttack();
    }

    private void UpdateAttack()
    {
        if (_player == null) { EnterDayIdle(); return; }

        if (DistanceToPlayer() > AttackRange * 1.1f) { EnterChase(); return; }
        if (Time.time < _nextAttackTime) return;

        _nextAttackTime = Time.time + AttackCooldown;

        // 근접만 플레이어를 바라봄
        if (_data == null || _data.attackType == EnemyAttackType.Melee)
            FaceTarget(_player.position);

        _animCtrl?.PlayAttack();
        _attack.Execute();
    }

    // State 진입
    private void EnterDayIdle()
    {
        _state = State.DayIdle;
        _stateEndTime = Time.time + Random.Range(DayIdleTime * 0.7f, DayIdleTime * 1.3f);
        _movement.Stop();
    }

    private void EnterDayPatrol()
    {
        _state = State.DayPatrol;
        _stateEndTime = Time.time + Random.Range(DayWalkTime * 0.7f, DayWalkTime * 1.3f);

        if (_movement.TryGetRandomPatrolPoint(transform.position, PatrolRadius, out var p))
            _movement.PatrolTo(p);
        else
            _movement.Stop();
    }

    private void EnterChase()
    {
        _state = State.Chase;
        // 첫 목적지는 UpdateChase에서 매 프레임 갱신
    }

    private void EnterAttack()
    {
        _state = State.Attack;
        _movement.Stop();
        _nextAttackTime = Mathf.Max(_nextAttackTime, Time.time);
    }

    private void EnterStun()
    {
        _state = State.Stun;
        _stateEndTime = Time.time + StunTime;
        _movement.Stop();
        _animCtrl?.PlayStun();
    }

    private void EnterChaseOrDay()
    {
        if (DayNightManager.Instance.IsNight || _aggro) EnterChase();
        else EnterDayIdle();
    }

    // 외부 호출
    public void OnDamaged(Vector3 hitPoint, bool stun)
    {
        if (_state == State.Dead) return;

        _aggro = true;
        _forcedAggroUntil = Time.time + DamageAggroHold;

        if (stun) EnterStun();
        else if (_state == State.DayIdle || _state == State.DayPatrol) EnterChase();
    }

    public void Die()
    {
        if (_state == State.Dead) return;

        _state = State.Dead;
        _movement.Stop();
        _animCtrl?.SetDead(true);

        DisableColliders();
        StartCoroutine(CoDeathRoutine());
    }

    // 유틸
    private float DistanceToPlayer()
    {
        if (_player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, _player.position);
    }

    private void FaceTarget(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 10f);
    }

    private void DisableColliders()
    {
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    // 코루틴
    private IEnumerator CoRandomStart()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));
        EnterDayIdle();
    }

    private IEnumerator CoBindPlayer()
    {
        while (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                _player = go.transform;
                _attack.UpdateTarget(_player);
            }
            yield return null;
        }
    }

    private IEnumerator CoDeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        float fadeTime = 1.5f;
        float t = 0f;

        var renderers = GetComponentsInChildren<Renderer>(true);

        // 각 Renderer Material 인스턴스화
        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = new Material(mats[i]);
            r.materials = mats;
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

            foreach (var r in renderers)
            {
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        var c = m.GetColor("_BaseColor"); c.a = alpha;
                        m.SetColor("_BaseColor", c);
                    }
                    if (m.HasProperty("_Color"))
                    {
                        var c = m.GetColor("_Color"); c.a = alpha;
                        m.SetColor("_Color", c);
                    }
                }
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}