using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

public class EnergyBall : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    private PoolKey _poolKey;
    private float _despawnTime;
    private int _damage;
    private Transform _owner;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_rb == null) _rb = GetComponentInChildren<Rigidbody>();
    }

    private void OnEnable()
    {
        if (_rb != null) _rb.linearVelocity = Vector3.zero;
    }

    public void Init(PoolKey poolKey, Transform owner, Vector3 dir, float speed, int damage, float lifeTime)
    {
        _poolKey = poolKey;
        _owner = owner;
        _damage = damage;

        _despawnTime = Time.time + lifeTime;

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        transform.forward = dir;

        if (_rb != null)
            _rb.linearVelocity = dir * speed;
    }

    private void Update()
    {
        if (Time.time >= _despawnTime)
            Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<EnergyBall>() != null) return;
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
        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.Despawn(_poolKey, gameObject);
        else
            gameObject.SetActive(false);
    }
}