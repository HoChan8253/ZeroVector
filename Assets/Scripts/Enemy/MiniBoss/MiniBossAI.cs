using UnityEngine;
using System.Collections;

public class MiniBossAI : MonoBehaviour, IEnemyAI
{
    private enum State { DayIdle, DayPatrol, Chase, Attack1, Attack2, Stomp, Taunt, Stun, Dead }

    [Header("Refs")]
    [SerializeField] private Transform _player;
    [SerializeField] private MiniBossData _data;
    [SerializeField] private BossAnimation _animCtrl;
    [SerializeField] private EnemyMovement _movement;
    [SerializeField] private BossAttack _attack;

    public int MaxHp => _data != null ? _data.maxHp : 1000;
    public int MaxShield => _data != null ? _data.maxShield : 0;
    public bool UseShield => _data != null && _data.useShield;
    public bool CanStun => _data != null ? _data.canStun : true;
    public float StunTime => _data != null ? _data.stunTime : 1.5f;
    public float HeadshotMultiplier => _data != null ? _data.headshotMultiplier : 1f;
    public bool IsDead => _state == State.Dead;

    public MiniBossData BossData => _data;

    private float AggroRange => _data != null ? _data.aggroRange : 20f;
    private float DeaggroRange => _data != null ? _data.deaggroRange : 30f;
    private float DamageAggroHold => _data != null ? _data.damageAggroHoldTime : 5f;
    private float DayIdleTime => _data != null ? _data.dayIdleTime : 3f;
    private float DayWalkTime => _data != null ? _data.dayWalkTime : 4f;
    private float PatrolRadius => _data != null ? _data.patrolRadius : 8f;

    // Runtime
    private State _state;
    private float _stateEndTime;
    private float _forcedAggroUntil;
    private bool _aggro;

    private float _nextAtk1Time;
    private float _nextAtk2Time;
    private float _nextStompTime;
    private float _nextTauntTime;

    private EnemyHealth _health;

    private void Awake()
    {
        if (_movement == null) _movement = GetComponent<EnemyMovement>();
        if (_attack == null) _attack = GetComponent<BossAttack>();
        if (_animCtrl == null) _animCtrl = GetComponentInChildren<BossAnimation>(true);
        _health = GetComponent<EnemyHealth>();

        InitMovement();
        _attack.Init(_data, _player);
    }

    private void Start() => StartCoroutine(CoRandomStart());

    private void OnEnable()
    {
        if (_player == null) StartCoroutine(CoBindPlayer());
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

        if (Time.time < _forcedAggroUntil) { _aggro = true; return; }
        if (DayNightManager.Instance != null && DayNightManager.Instance.IsNight)
        { _aggro = true; return; }

        float dist = Dist();
        if (!_aggro) { if (dist <= AggroRange) _aggro = true; }
        else
        {
            if (dist >= DeaggroRange)
            {
                _aggro = false;
                if (_state == State.Chase || IsAttackState()) EnterDayIdle();
            }
        }
    }

    // State Machine
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

            case State.Attack1:
            case State.Attack2:
            case State.Stomp:
            case State.Taunt:
                if (Time.time >= _stateEndTime) EnterChase();
                break;

            case State.Stun:
                if (Time.time >= _stateEndTime)
                {
                    _animCtrl?.SetStun(false);
                    EnterChaseOrDay();
                }
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

        // Taunt > Stomp > Attack2 > Attack1
        float dist = Dist();

        if (CanTaunt()) { EnterTaunt(); return; }
        if (CanStomp(dist)) { EnterStomp(); return; }
        if (CanAtk2(dist)) { EnterAttack2(); return; }
        if (CanAtk1(dist)) { EnterAttack1(); return; }
    }

    private bool CanAtk1(float dist) => dist <= (_data?.atk1Range ?? 2.5f) && Time.time >= _nextAtk1Time;
    private bool CanAtk2(float dist) => dist <= (_data?.atk2Range ?? 4f) && Time.time >= _nextAtk2Time;
    private bool CanStomp(float dist) => dist <= (_data?.stompRange ?? 8f) && Time.time >= _nextStompTime;
    private bool CanTaunt()
    {
        if (Time.time < _nextTauntTime) return false;
        if (_health == null) return false;
        return !_health.HasShield;   // 실드가 없을 때만 Taunt
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
    }

    private void EnterAttack1()
    {
        _state = State.Attack1;
        _movement.Stop();
        FaceTarget();
        _nextAtk1Time = Time.time + (_data?.atk1Cooldown ?? 2f);
        _stateEndTime = Time.time + 2.9f; // 애니메이션 길이 여유값
        _animCtrl?.PlayAttack1();
    }

    private void EnterAttack2()
    {
        _state = State.Attack2;
        _movement.Stop();
        _nextAtk2Time = Time.time + (_data?.atk2Cooldown ?? 5f);
        _stateEndTime = Time.time + 2.1f;
        _animCtrl?.PlayAttack2();
    }

    private void EnterStomp()
    {
        _state = State.Stomp;
        _movement.Stop();
        _nextStompTime = Time.time + (_data?.stompCooldown ?? 8f);
        _stateEndTime = Time.time + 2.1f;
        _animCtrl?.PlayStomp();
    }

    private void EnterTaunt()
    {
        _state = State.Taunt;
        _movement.Stop();
        _nextTauntTime = Time.time + (_data?.shieldCooldown ?? 30f);
        _stateEndTime = Time.time + 2.5f; // Taunt 애니메이션 길이 여유값

        // 실드 즉시 회복
        _health.ResetShieldOnly();
        _animCtrl?.PlayTaunt();
    }

    private void EnterStun()
    {
        _state = State.Stun;
        _stateEndTime = Time.time + (StunTime);
        _movement.Stop();
        _animCtrl?.SetStun(true);
    }

    private void EnterChaseOrDay()
    {
        bool isNight = DayNightManager.Instance != null && DayNightManager.Instance.IsNight;
        if (isNight || _aggro) EnterChase();
        else EnterDayIdle();
    }

    public void OnDamaged(Vector3 hitPoint, bool stun)
    {
        if (_state == State.Dead) return;

        _aggro = true;
        _forcedAggroUntil = Time.time + DamageAggroHold;

        if (stun)
        {
            EnterStun();
        }
        else if (_state == State.DayIdle || _state == State.DayPatrol)
        {
            EnterChase();
        }
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

    public void ActivateCombat()
    {
        if (_state == State.Dead) return;
        _aggro = true;
        if (_state == State.DayIdle || _state == State.DayPatrol)
            EnterChase();
    }

    // 유틸
    private float Dist() =>
        _player != null ? Vector3.Distance(transform.position, _player.position) : float.MaxValue;

    private bool IsAttackState() =>
        _state == State.Attack1 || _state == State.Attack2 ||
        _state == State.Stomp || _state == State.Taunt;

    private void FaceTarget()
    {
        if (_player == null) return;
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void DisableColliders()
    {
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
    }

    private void InitMovement()
    {
        // NavMeshAgent 직접 세팅
        _movement.Init(null);
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && _data != null)
        {
            agent.speed = _data.chaseSpeed;
            agent.angularSpeed = _data.angularSpeed;
            agent.acceleration = _data.acceleration;
        }
    }

    // 코루틴
    private IEnumerator CoRandomStart()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
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

        float fadeTime = 2f;
        float t = 0f;
        var renderers = GetComponentsInChildren<Renderer>(true);

        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++) mats[i] = new Material(mats[i]);
            r.materials = mats;
        }

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            foreach (var r in renderers)
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_BaseColor")) { var c = m.GetColor("_BaseColor"); c.a = alpha; m.SetColor("_BaseColor", c); }
                    if (m.HasProperty("_Color")) { var c = m.GetColor("_Color"); c.a = alpha; m.SetColor("_Color", c); }
                }
            yield return null;
        }

        Destroy(gameObject);
    }
}