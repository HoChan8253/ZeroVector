using UnityEngine;

public class BossProjHoming : MonoBehaviour
{
    private PoolKey _poolKey;
    private Transform _target;
    private float _speed;
    private float _turnSpeed;
    private float _despawnTime;
    private int _damage;
    private Transform _owner;

    public void Init(PoolKey poolKey, Transform owner, Transform target,
                     float speed, float turnSpeed, int damage, float lifeTime)
    {
        _poolKey = poolKey;
        _owner = owner;
        _target = target;
        _speed = speed;
        _turnSpeed = turnSpeed;
        _damage = damage;
        _despawnTime = Time.time + lifeTime;
    }

    private void Update()
    {
        if (Time.time >= _despawnTime)
        { Despawn(); return; }

        // 유도
        if (_target != null)
        {
            Vector3 toTarget = (_target.position + Vector3.up * 0.5f - transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, toTarget,
                _turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
            transform.forward = newDir;
        }

        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_owner != null && other.transform.IsChildOf(_owner)) return;

        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(_damage);
            Despawn();
            return;
        }

        Despawn();
    }

    private void Despawn()
    {
        ObjectPoolManager.Instance?.Despawn(_poolKey, gameObject);
    }
}