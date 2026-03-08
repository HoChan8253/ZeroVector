using UnityEngine;
using System.Collections;

// 공중 유닛 전용 AI
// NavMesh 를 사용하지 않고 Transform 직접 제어
public class FlyingEnemyAI : MonoBehaviour, IEnemyAI
{
    private enum State { Spawning, Idle, Chase, Strafe, Dead }

    [Header("Refs")]
    [SerializeField] private EnemyData _data;
    [SerializeField] private FlyingEnemyMovement _movement;
    [SerializeField] private FlyingEnemyAttack _attack;
    [SerializeField] private Animator _animator;

    [Header("Animator")]
    [SerializeField] private string _paramAttack = "Attack";
    [SerializeField] private string _paramDead = "Dead";
    [SerializeField] private string _paramEncounter = "Encounter";

    [Header("Death Fall")]
    [Tooltip("사망 애니메이션 재생 후 낙하 시작까지 대기 시간")]
    [SerializeField] private float _deathAnimDelay = 0.6f;
    [Tooltip("지면까지 낙하하는 데 걸리는 시간")]
    [SerializeField] private float _deathFallTime = 1.2f;
    [Tooltip("낙하 속도 곡선")]
    [SerializeField]
    private AnimationCurve _deathFallCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, 0f),   // 처음엔 천천히
        new Keyframe(0.6f, 0.6f, -1f, -3f),
        new Keyframe(1f, 0f, -4f, 0f));  // 마지막엔 빠르게
    [Tooltip("페이드아웃 시작 시점")]
    [SerializeField] private float _fadeStartNormalized = 0.5f;

    // 외부 참조
    public EnemyData Data => _data;
    public bool IsDead => _state == State.Dead;

    // 데이터 캐시
    private float AggroRange => _data != null ? _data.aggroRange : 12f;
    private float DeaggroRange => _data != null ? _data.deaggroRange : 18f;
    private float AttackRange => _data != null ? _data.attackRange : 10f;
    private float AttackCooldown => _data != null ? _data.attackCooldown : 2f;
    private float DamageAggroHold => _data != null ? _data.damageAggroHoldTime : 2f;
    private float FlyHeight => _data != null ? _data.flyingHeight : 4.5f;

    // Runtime
    private Transform _player;
    private State _state;
    private bool _aggro;
    private float _forcedAggroUntil;
    private float _nextAttackTime;

    private void Awake()
    {
        if (_movement == null) _movement = GetComponent<FlyingEnemyMovement>();
        if (_attack == null) _attack = GetComponent<FlyingEnemyAttack>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>(true);

        _movement.Init(_data, FlyHeight);
    }

    private void Start()
    {
        StartCoroutine(CoSpawn());
    }

    private void OnEnable()
    {
        if (_player == null)
            StartCoroutine(CoBindPlayer());
    }

    private void Update()
    {
        if (_state == State.Dead || _state == State.Spawning) return;

        UpdateAggro();
        UpdateState();
    }

    // Aggro
    private void UpdateAggro()
    {
        if (_player == null) return;

        // 야간 or 피격 후 강제 어그로
        if (DayNightManager.Instance != null && DayNightManager.Instance.IsNight)
        { _aggro = true; return; }

        if (Time.time < _forcedAggroUntil)
        { _aggro = true; return; }

        float dist = DistToPlayer();
        if (!_aggro)
        {
            if (dist <= AggroRange) _aggro = true;
        }
        else
        {
            if (dist >= DeaggroRange)
            {
                _aggro = false;
                if (_state == State.Chase || _state == State.Strafe)
                    EnterIdle();
            }
        }
    }

    private void UpdateState()
    {
        switch (_state)
        {
            case State.Idle:
                if (_aggro) EnterChase();
                else _movement.HoverInPlace();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Strafe:
                UpdateStrafe();
                break;
        }
    }

    private void UpdateChase()
    {
        if (_player == null) { EnterIdle(); return; }

        _movement.MoveTowardPlayer(_player.position);

        if (DistToPlayer() <= AttackRange)
            EnterStrafe();
    }

    private void UpdateStrafe()
    {
        if (_player == null) { EnterIdle(); return; }

        // 공격 사거리 이탈 → 다시 추격
        if (DistToPlayer() > AttackRange * 1.2f)
        { EnterChase(); return; }

        // 플레이어 주위를 선회
        _movement.StrafeAroundPlayer(_player.position);

        // 공격 쿨다운
        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + AttackCooldown;
            TriggerAttack();
        }
    }

    // State 진입
    private void EnterIdle()
    {
        _state = State.Idle;
        _movement.HoverInPlace();
    }

    private void EnterChase()
    {
        _state = State.Chase;
    }

    private void EnterStrafe()
    {
        _state = State.Strafe;
        _nextAttackTime = Time.time + 0.3f;
    }

    // 공격
    private void TriggerAttack()
    {
        _animator?.SetTrigger(_paramAttack);
        _attack.Execute(_player);
    }

    public void OnDamaged(Vector3 hitPoint, bool stun)
    {
        if (_state == State.Dead) return;

        _aggro = true;
        _forcedAggroUntil = Time.time + DamageAggroHold;

        if (_state == State.Idle) EnterChase();
    }

    public void Die()
    {
        if (_state == State.Dead) return;

        _state = State.Dead;
        _movement.Stop();
        _animator?.SetTrigger(_paramDead);

        StartCoroutine(CoDeathRoutine());
    }

    // 유틸
    private float DistToPlayer()
    {
        if (_player == null) return float.MaxValue;

        Vector3 flat = _player.position - transform.position;
        flat.y = 0f;
        return flat.magnitude;
    }

    private void DisableColliders()
    {
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    // 등장 연출 후 Idle 진입
    private IEnumerator CoSpawn()
    {
        _state = State.Spawning;
        _animator?.SetTrigger(_paramEncounter);

        yield return new WaitForSeconds(1.5f);

        EnterIdle();
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
        yield return new WaitForSeconds(_deathAnimDelay);

        float startY = transform.position.y;

        DisableColliders();

        int groundMask = LayerMask.GetMask("Ground", "Terrain", "Default");
        float groundY = startY - FlyHeight;

        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 50f, groundMask))
            groundY = hit.point.y;

        if (startY - groundY < 0.5f)
            groundY = startY - 2f;

        // Material 인스턴스화
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = new Material(mats[i]);
            r.materials = mats;
        }

        float t = 0f;
        while (t < _deathFallTime)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / _deathFallTime);

            // 낙하
            float curveY = _deathFallCurve.Evaluate(normalized);
            var pos = transform.position;
            pos.y = Mathf.Lerp(groundY, startY, curveY);
            transform.position = pos;

            // 페이드아웃
            float fadeProgress = Mathf.InverseLerp(_fadeStartNormalized, 1f, normalized);
            float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            ApplyAlpha(renderers, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    private static void ApplyAlpha(Renderer[] renderers, float alpha)
    {
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
    }
}