using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossAI : MonoBehaviour, IEnemyAI
{
    private enum State
    {
        Idle,
        Run,
        RunAim,
        Smack,
        Shoot,
        ShootTriple,
        Slam,
        Dead
    }

    [Header("Refs")]
    [SerializeField] private BossData _data;
    [SerializeField] private BossAnimCtrl _animCtrl;
    [SerializeField] private BossAttackCtrl _attackCtrl;
    [SerializeField] private Transform _slamCenter;    // 반피 Slam 위치
    [SerializeField] private Transform _player;

    public int MaxHp => _data != null ? _data.maxHp : 3000;
    public int MaxShield => 0;
    public bool UseShield => false;
    public bool CanStun => false;
    public float StunTime => 0f;
    public bool IsDead => _state == State.Dead;
    public float HeadshotMultiplier => 1f;

    private State _state;
    private float _stateEndTime;
    private bool _isPhase2;
    private bool _phase2Triggered;

    private float _nextSmackTime;
    private float _nextShootTime;
    private float _nextTripleTime;
    private float _nextSlamTime;
    private float _nextIdleTime;

    private NavMeshAgent _agent;
    private EnemyHealth _health;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();

        if (_animCtrl == null) _animCtrl = GetComponentInChildren<BossAnimCtrl>(true);
        if (_attackCtrl == null) _attackCtrl = GetComponentInChildren<BossAttackCtrl>(true);

        if (_data != null && _agent != null)
        {
            _agent.speed = _data.chaseSpeed;
            _agent.angularSpeed = _data.angularSpeed;
            _agent.acceleration = _data.acceleration;
        }
    }

    private void Start()
    {
        if (_player == null)
            StartCoroutine(CoBindPlayer());

        _attackCtrl?.Init(_data, _player, this);

        // 등장 연출은 나중에 코루틴으로 처리 예정
        // 지금은 바로 전투 시작
        EnterRun();
    }

    private void Update()
    {
        if (_state == State.Dead) return;
        CheckPhase2();
        UpdateState();
        UpdateAnim();
    }

    // 페이즈 체크
    private void CheckPhase2()
    {
        if (_phase2Triggered) return;
        if (_health == null) return;

        float ratio = (float)_health.Hp / MaxHp;
        if (ratio > _data.phase2HpRatio) return;

        _phase2Triggered = true;
        _isPhase2 = true;
        _animCtrl?.SetHalfHp(true);
        EnterSlam();
    }

    // 상태머신
    private void UpdateState()
    {
        switch (_state)
        {
            case State.Run: UpdateRun(); break;
            case State.RunAim: UpdateRunAim(); break;
            case State.Idle:
            case State.Smack:
            case State.Shoot:
            case State.ShootTriple:
            case State.Slam:
                if (Time.time >= _stateEndTime) EnterRun();
                break;
        }
    }

    private void UpdateRun()
    {
        if (_player == null) return;
        float dist = Dist();

        // 가까우면 Smack
        if (dist <= _data.smackRange && Time.time >= _nextSmackTime)
        { EnterSmack(); return; }

        // 멀면 RunAim으로 전환
        if (dist >= _data.farRange)
        { EnterRunAim(); return; }

        // 중거리 패턴 선택
        if (CanPickPattern())
        {
            PickAndEnterPattern();
            return;
        }

        _agent.speed = _data.chaseSpeed;
        _agent.SetDestination(_player.position);
    }

    private void UpdateRunAim()
    {
        if (_player == null) return;
        float dist = Dist();

        // 가까워지면 Run으로 복귀
        if (dist < _data.farRange * 0.7f)
        { EnterRun(); return; }

        // 이동하면서 추격
        _agent.speed = _data.aimMoveSpeed;
        _agent.SetDestination(_player.position);
        FaceTarget();

        // 사격 가능하면 발사
        if (Time.time >= _nextShootTime)
        {
            _nextShootTime = Time.time + _data.shootCooldown;
            _stateEndTime = Time.time + 2f;
            _animCtrl?.SetRanged(true);
            // 실제 발사는 AE_ShootFire 애니메이션 이벤트로 처리
        }
    }

    // 보스 패턴
    private bool CanPickPattern()
    {
        bool shootReady = Time.time >= _nextShootTime;
        bool tripleReady = Time.time >= _nextTripleTime;
        bool slamReady = _isPhase2 && Time.time >= _nextSlamTime;
        bool idleReady = Time.time >= _nextIdleTime;
        return shootReady || tripleReady || slamReady || idleReady;
    }

    private void PickAndEnterPattern()
    {
        // 2페이즈 Slam 우선
        if (_isPhase2 && Time.time >= _nextSlamTime)
        { EnterSlam(); return; }

        // Idle 랜덤 삽입
        if (Time.time >= _nextIdleTime && Random.value < _data.idleChance)
        { EnterIdle(); return; }

        // Shoot / ShootTriple 중 가능한 것 선택
        bool shootReady = Time.time >= _nextShootTime;
        bool tripleReady = Time.time >= _nextTripleTime;

        if (shootReady && tripleReady)
        {
            if (Random.value < 0.5f) EnterShoot();
            else EnterShootTriple();
        }
        else if (shootReady) EnterShoot();
        else if (tripleReady) EnterShootTriple();
    }

    // State 진입
    private void EnterRun()
    {
        _state = State.Run;
        _animCtrl?.SetMoving(true);
        _animCtrl?.SetRanged(false);
        _agent.isStopped = false;
    }

    private void EnterRunAim()
    {
        _state = State.RunAim;
        _animCtrl?.SetMoving(true);
        _animCtrl?.SetRanged(true);
        _agent.isStopped = false;
    }

    private void EnterIdle()
    {
        _state = State.Idle;
        _stateEndTime = Time.time + _data.idleDuration;
        _nextIdleTime = Time.time + _data.idleDuration + Random.Range(5f, 10f);
        StopAgent();
        _animCtrl?.SetMoving(false);
        _animCtrl?.SetRanged(false);
    }

    private void EnterSmack()
    {
        _state = State.Smack;
        _stateEndTime = Time.time + 1.1f;
        _nextSmackTime = Time.time + _data.smackCooldown;
        StopAgent();
        FaceTarget();
        _animCtrl?.PlaySmack();
    }

    private void EnterShoot()
    {
        _state = State.Shoot;
        _stateEndTime = Time.time + 0.9f;
        _nextShootTime = Time.time + _data.shootCooldown;
        StopAgent();
        FaceTarget();
        _animCtrl?.SetMoving(false);
        _animCtrl?.SetRanged(false);
        _animCtrl?.PlayShoot();
    }

    private void EnterShootTriple()
    {
        _state = State.ShootTriple;
        _stateEndTime = Time.time + 0.9f;
        _nextTripleTime = Time.time + _data.tripleShootCooldown;
        StopAgent();
        FaceTarget();
        _animCtrl?.SetMoving(false);
        _animCtrl?.SetRanged(false);
        _animCtrl?.PlayShootTriple();
    }

    private void EnterSlam()
    {
        _state = State.Slam;
        _stateEndTime = Time.time + 2.9f;
        _nextSlamTime = Time.time + _data.slamCooldown;
        StopAgent();

        // Slam 위치로 순간 이동 or 이동은 나중에 코루틴으로 처리 가능
        if (_slamCenter != null)
            transform.position = _slamCenter.position;

        FaceTarget();
        _animCtrl?.PlaySlam();
    }

    public void OnDamaged(Vector3 hitPoint, bool stun) { }

    public void Die()
    {
        if (_state == State.Dead) return;
        _state = State.Dead;
        StopAgent();
        _animCtrl?.SetDead(true);
        StartCoroutine(CoDeathEffect());
    }

    public void ActivateCombat()
    {
        if (_state == State.Dead) return;
        EnterRun();
    }

    // Slam 투사체 발사 완료 후 BossAttackCtrl 에서 호출
    public void OnSlamFired()
    {
        // TODO: 잡몹 스폰
        Debug.Log("[BossAI] Slam 완료 → 잡몹 스폰");
    }

    // Death 연출
    private IEnumerator CoDeathEffect()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);

        // 재질 인스턴스화
        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = new Material(mats[i]);
            r.materials = mats;
        }

        float explodeTime = 3f;
        float t = 0f;

        while (t < explodeTime)
        {
            t += Time.deltaTime;

            // 랜덤 위치에서 폭발 파티클 (풀 있으면 Spawn, 없으면 생략)
            if (Random.value < Time.deltaTime * 8f)
            {
                Vector3 randomPos = transform.position
                    + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1.5f), Random.Range(-0.5f, 0.5f));
                // TODO: ObjectPoolManager.Instance.Spawn(PoolKey.Explosion, randomPos, Quaternion.identity);
                Debug.Log("[BossAI] 폭발 연출");
            }

            // 몸 색상을 붉게 물들이기
            float redRatio = Mathf.Clamp01(t / explodeTime);
            foreach (var r in renderers)
                foreach (var m in r.materials)
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        var c = m.GetColor("_BaseColor");
                        c.r = Mathf.Lerp(c.r, 1f, redRatio * Time.deltaTime * 5f);
                        c.g = Mathf.Lerp(c.g, 0f, redRatio * Time.deltaTime * 5f);
                        c.b = Mathf.Lerp(c.b, 0f, redRatio * Time.deltaTime * 5f);
                        m.SetColor("_BaseColor", c);
                    }
                }

            yield return null;
        }

        // 알파값 서서히 감소
        float fadeTime = 1.5f;
        float ft = 0f;
        while (ft < fadeTime)
        {
            ft += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, ft / fadeTime);
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
    
    // 유틸
    private float Dist() =>
        _player != null ? Vector3.Distance(transform.position, _player.position) : float.MaxValue;

    private void FaceTarget()
    {
        if (_player == null) return;
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void StopAgent()
    {
        _animCtrl?.SetMoving(false);
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
    }

    private void UpdateAnim()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
        _animCtrl?.SetMoving(isMoving);
    }

    private IEnumerator CoBindPlayer()
    {
        while (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                _player = go.transform;
                _attackCtrl?.UpdateTarget(_player);
            }
            yield return null;
        }
    }
}