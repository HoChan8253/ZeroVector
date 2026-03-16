using UnityEngine;

public class BossProjStraight : MonoBehaviour
{
    private PoolKey _poolKey;
    private Vector3 _dir;
    private float _speed;
    private float _despawnTime;
    private int _damage;
    private Transform _owner;

    public void Init(PoolKey poolKey, Transform owner, Vector3 dir, float speed, int damage, float lifeTime)
    {
        _poolKey = poolKey;
        _owner = owner;
        _dir = dir.normalized;
        _speed = speed;
        _damage = damage;
        _despawnTime = Time.time + lifeTime;

        transform.forward = _dir;
    }

    private void Update()
    {
        transform.position += _dir * _speed * Time.deltaTime;

        if (Time.time >= _despawnTime)
            Despawn();
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