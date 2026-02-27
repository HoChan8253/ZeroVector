using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private bool _stunnable = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem _hitFx;

    [SerializeField] private EnemyData _data;

    private int _hp;
    private EnemyAI _ai;

    private void Awake()
    {
        _hp = _data != null ? _data.maxHp : _maxHp;
        _ai = GetComponent<EnemyAI>();
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

    private void PlayHitFx(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_hitFx == null) return;

        _hitFx.transform.SetPositionAndRotation(hitPoint, Quaternion.LookRotation(hitNormal));
        _hitFx.Play(true);
    }
}