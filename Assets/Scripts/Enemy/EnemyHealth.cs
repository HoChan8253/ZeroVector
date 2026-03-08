using UnityEngine;
using System.Collections;

// EnemyAI / FlyingEnemyAI 양쪽 모두에서 사용 가능 체력 컴포넌트
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private bool _resetHpOnEnable = true;

    [Header("Stun")]
    [SerializeField] private bool _stunnable = true;

    [Header("Shield Visual")]
    [SerializeField] private GameObject _shieldVisual;

    // Runtime
    private int _hp;
    private int _shield;
    private IEnemyAI _ai;

    public int Hp => _hp;
    public int Shield => _shield;
    public int MaxHp => _ai?.Data != null ? _ai.Data.maxHp : 100;
    public int MaxShield
    {
        get
        {
            if (_ai?.Data != null && _ai.Data.useShield)
                return _ai.Data.maxShield;
            return 0;
        }
    }
    public bool HasShield => _shield > 0;

    private void Awake()
    {
        // GetComponent 로 어느 AI 타입이든 자동으로 찾음
        _ai = GetComponent<IEnemyAI>();

        if (_ai == null)
            Debug.LogWarning($"[EnemyHealth] {name}: IEnemyAI 구현체를 찾지 못했습니다. " +
                             "EnemyAI 또는 FlyingEnemyAI 컴포넌트가 같은 GameObject에 있어야 합니다.");

        ResetHp();
    }

    private void OnEnable()
    {
        if (_resetHpOnEnable)
            ResetHp();
    }

    public void ResetHp()
    {
        _hp = MaxHp;
        _shield = MaxShield;
        RefreshShieldVisual();
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, false, transform.position, Vector3.up);
    }

    public void TakeDamage(int amount, bool headshot, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_ai != null && _ai.IsDead) return;
        if (amount <= 0) return;

        PlayHitFx(hitPoint, hitNormal);

        // 실드가 남아 있으면 실드 우선 차감
        if (HasShield)
        {
            _shield -= amount;
            if (_shield <= 0)
            {
                _shield = 0;
                RefreshShieldVisual();
                OnShieldBreak();
            }
            // 실드에 맞았을 때는 스턴 없이 어그로만 활성화
            _ai?.OnDamaged(hitPoint, false);
            return;
        }

        // HP 차감
        _hp -= amount;

        bool stun = CanStun() && headshot;
        _ai?.OnDamaged(hitPoint, stun);

        if (_hp <= 0)
        {
            _hp = 0;
            _ai?.Die();
        }
    }

    // 외부 호출
    private bool CanStun()
    {
        if (HasShield) return false;

        // EnemyData 에 canStun 있으면 우선 사용
        if (_ai?.Data != null) return _ai.Data.canStun;

        return _stunnable;
    }

    private void OnShieldBreak()
    {
        Debug.Log($"[EnemyHealth] {name} 실드 파괴");
    }

    private void RefreshShieldVisual()
    {
        if (_shieldVisual != null)
            _shieldVisual.SetActive(HasShield);
    }

    // HitFX
    public void PlayHitFx(Vector3 point, Vector3 normal)
    {
        if (ObjectPoolManager.Instance == null) return;

        Quaternion rot = normal != Vector3.zero
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;

        Vector3 pos = point + normal * 0.01f;
        GameObject obj = ObjectPoolManager.Instance.Spawn(PoolKey.HitFx_ElectricShort, pos, rot);
        if (obj == null) return;

        var ps = obj.GetComponent<ParticleSystem>()
                 ?? obj.GetComponentInChildren<ParticleSystem>(true);

        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
            ps.Play(true);
            float life = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(CoDespawnFx(PoolKey.HitFx_ElectricShort, obj, life));
        }
        else
        {
            ObjectPoolManager.Instance.Despawn(PoolKey.HitFx_ElectricShort, obj);
        }
    }

    private IEnumerator CoDespawnFx(PoolKey key, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ObjectPoolManager.Instance != null && obj != null)
            ObjectPoolManager.Instance.Despawn(key, obj);
    }
}