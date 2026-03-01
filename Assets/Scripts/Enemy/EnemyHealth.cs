using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private bool _stunnable = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem _hitFxPrefab;

    private int _hp;
    private EnemyAI _ai;

    private void Awake()
    {
        _ai = GetComponent<EnemyAI>();
        var data = (_ai != null) ? _ai.Data : null;
        _hp = (data != null) ? data.maxHp : _maxHp;
    }

    public void TakeDamage(int amount, bool headshot, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_hp <= 0) return;

        PlayHitFx(hitPoint, hitNormal);

        _hp -= amount;

        bool stun = _stunnable && headshot;
        if (_ai != null) _ai.OnDamaged(hitPoint, stun);

        if (_hp <= 0 && _ai != null)
            _ai.Die();
    }

    public void PlayHitFx(Vector3 point, Vector3 normal)
    {
        if (_hitFxPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(normal);

        var fx = Instantiate(_hitFxPrefab, point + normal * 0.01f, rot);

        fx.gameObject.SetActive(true);

        fx.Clear(true);
        fx.Play(true);

        float life = fx.main.duration + fx.main.startLifetime.constantMax;
        Destroy(fx.gameObject, life);
    }
}