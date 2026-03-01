using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private bool _resetHpOnEnable = true;

    [Header("Stun")]
    [SerializeField] private bool _stunnable = true;

    private int _hp;
    private EnemyAI _ai;

    public int Hp => _hp;
    public int MaxHp => (_ai != null && _ai.Data != null) ? _ai.Data.maxHp : 100;

    private void Awake()
    {
        _ai = GetComponent<EnemyAI>();
        ResetHp();
    }

    private void OnEnable()
    {
        if (_resetHpOnEnable)
            ResetHp();
    }

    private bool CanStun()
    {
        if (_ai != null && _ai.Data != null) return _ai.Data.canStun;
        return _stunnable;
    }

    public void ResetHp()
    {
        _hp = MaxHp;
    }

    public void TakeDamage(int amount, bool headshot, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_hp <= 0) return;

        PlayHitFx(hitPoint, hitNormal);

        _hp -= amount;

        bool stun = CanStun() && headshot;
        if (_ai != null) _ai.OnDamaged(hitPoint, stun);

        if (_hp <= 0 && _ai != null)
            _ai.Die();
    }

    public void PlayHitFx(Vector3 point, Vector3 normal)
    {
        if (ObjectPoolManager.Instance == null) return;

        Quaternion rot = Quaternion.LookRotation(normal);
        Vector3 pos = point + normal * 0.01f;

        GameObject obj = ObjectPoolManager.Instance.Spawn(PoolKey.HitFx_ElectricShort, pos, rot);
        if (obj == null) return;

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps == null) ps = obj.GetComponentInChildren<ParticleSystem>(true);

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