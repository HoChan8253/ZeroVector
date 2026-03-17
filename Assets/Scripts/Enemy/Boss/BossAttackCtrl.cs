using UnityEngine;
using System.Collections;

public class BossAttackCtrl : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private LayerMask _playerMask;

    private BossData _data;
    private Transform _target;
    private BossAI _ai;

    public void Init(BossData data, Transform target, BossAI ai)
    {
        _data = data;
        _target = target;
        _ai = ai;
    }

    public void UpdateTarget(Transform t) => _target = t;

    public void AE_SmackHit()
    {
        if (_target == null) return;
        float dist = Vector3.Distance(transform.position, _target.position);
        if (dist > _data.smackRange * 1.3f) return;

        var dmg = _target.GetComponentInParent<IDamageable>();
        dmg?.TakeDamage(_data.smackDamage);

        // 넉백
        var moveCC = _target.GetComponentInParent<PlayerMoveCC>();
        if (moveCC != null)
        {
            Vector3 dir = (_target.position - transform.position);
            dir.y = 0.3f;
            moveCC.ApplyKnockback(dir, _data.smackKnockback);
        }
    }

    public void AE_ShootFire()
    {
        if (_target == null || ObjectPoolManager.Instance == null) return;
        Vector3 origin = _muzzle != null ? _muzzle.position : transform.position + Vector3.up * 1.5f;
        Vector3 dir = (_target.position + Vector3.up * 0.5f - origin).normalized;

        var obj = ObjectPoolManager.Instance.Spawn(PoolKey.BossProj_Straight, origin, Quaternion.LookRotation(dir));
        obj?.GetComponent<BossProjStraight>()?.Init(
            PoolKey.BossProj_Straight, transform, dir,
            _data.shootProjSpeed, _data.shootDamage, _data.shootProjLife);
    }

    public void AE_ShootTripleFire() => StartCoroutine(CoTripleShot());

    private IEnumerator CoTripleShot()
    {
        for (int i = 0; i < 3; i++)
        {
            if (ObjectPoolManager.Instance == null) yield break;
            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position + Vector3.up * 1.5f;

            var obj = ObjectPoolManager.Instance.Spawn(
                PoolKey.BossProj_Homing, origin, transform.rotation);
            obj?.GetComponent<BossProjHoming>()?.Init(
                PoolKey.BossProj_Homing, transform, _target,
                _data.tripleShootProjSpeed,
                _data.tripleShootTurnSpeed,
                _data.tripleShootDamage,
                _data.tripleShootProjLife);

            yield return new WaitForSeconds(_data.tripleShootInterval);
        }
    }
}