using System;
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
    public int MaxHp => _ai != null ? _ai.MaxHp : 100;
    public int MaxShield => _ai != null && _ai.UseShield ? _ai.MaxShield : 0;
    public bool HasShield => _shield > 0;

    public event Action OnDead;

    public event Action<int, int> OnHpChanged;

    private void Awake()
    {
        // GetComponent 로 어느 AI 타입이든 자동으로 찾음
        _ai = GetComponent<IEnemyAI>();

        if (_ai == null)

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

    public void ResetShieldOnly()
    {
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

        // 헤드샷 배율 적용
        if (headshot && _ai != null)
            amount = Mathf.RoundToInt(amount * _ai.HeadshotMultiplier);

        // HP 차감
        _hp -= amount;
        OnHpChanged?.Invoke(_hp, MaxHp);

        bool stun = CanStun() && headshot;
        _ai?.OnDamaged(hitPoint, stun);

        if (_hp <= 0)
        {
            _hp = 0;
            GiveGoldReward();
            OnDead?.Invoke();
            _ai?.Die();

            GameStatsManager.Instance?.AddKill();
        }
    }

    private void GiveGoldReward()
    {
        if (GoldManager.Instance == null) return;

        int amount = 0;

        // EnemyAI
        if (TryGetComponent<EnemyAI>(out var enemyAI) && enemyAI.Data != null)
        {
            var d = enemyAI.Data;
            amount = d.randomGold
                ? UnityEngine.Random.Range(d.goldRewardMin, d.goldRewardMax + 1)
                : d.goldReward;
        }
        // MiniBossAI
        else if (TryGetComponent<MiniBossAI>(out var bossAI))
        {
            var d = bossAI.BossData;
            if (d != null)
                amount = d.randomGold
                    ? UnityEngine.Random.Range(d.goldRewardMin, d.goldRewardMax + 1)
                    : d.goldReward;
        }
        // FlyingEnemyAI
        else if (TryGetComponent<FlyingEnemyAI>(out var flyAI) && flyAI.Data != null)
        {
            var d = flyAI.Data;
            amount = d.randomGold
                ? UnityEngine.Random.Range(d.goldRewardMin, d.goldRewardMax + 1)
                : d.goldReward;
        }
        // Boss
        else if (TryGetComponent<BossAI>(out var newBossAI))
        {
            // BossData에 골드 필드 추가 후 처리
            // 지금은 고정값으로 임시 처리
            amount = 500;
        }

        if (amount > 0)
            GoldManager.Instance.Add(amount, transform.position);

        ItemDropSpawner.Instance?.TrySpawn(transform.position);

        SoundType deathSound = TryGetComponent<MiniBossAI>(out _)
        ? SoundType.EnemyDead_MiniBoss
        : SoundType.EnemyDead;
        PlaySfxAtPosition(deathSound);
    }

    private void PlaySfxAtPosition(SoundType sound)
    {
        if (SFXManager.Instance == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = transform.position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.spatialBlend = 1f; // 3D
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 5f;
        source.maxDistance = 30f;

        SFXManager.PlaySound(sound, source);

        Destroy(tempGO, 3f);
    }

    // 외부 호출
    private bool CanStun()
    {
        if (HasShield) return false;
        if (_ai != null) return _ai.CanStun;
        return _stunnable;
    }

    private void OnShieldBreak()
    {
        
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

    public void ForceKill()
    {
        if (_ai != null && _ai.IsDead) return;
        _shield = 0;
        RefreshShieldVisual();
        _hp = 0;
        OnHpChanged?.Invoke(_hp, MaxHp);
        GiveGoldReward();
        OnDead?.Invoke();
        _ai?.Die();
        GameStatsManager.Instance?.AddKill();
    }

    private IEnumerator CoDespawnFx(PoolKey key, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ObjectPoolManager.Instance != null && obj != null)
            ObjectPoolManager.Instance.Despawn(key, obj);
    }
}