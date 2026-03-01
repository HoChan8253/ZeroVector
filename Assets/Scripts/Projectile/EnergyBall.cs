using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

public class EnergyBall : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    private GameObject _poolKeyPrefab;
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

    public void Init(GameObject poolKeyPrefab, Transform owner, Vector3 dir, float speed, int damage, float lifeTime)
    {
        _poolKeyPrefab = poolKeyPrefab;
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
        // 투사체끼리 충돌 방지
        if (other.GetComponentInParent<EnergyBall>() != null) return;

        // 발사자 충돌 방지
        if (_owner != null && other.transform.IsChildOf(_owner)) return;

        // 플레이어 쪽에 IDamageable 달려있으면 데미지
        var dmg = other.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(_damage);
            Despawn();
            return;
        }

        // 아무거나 충돌해도 제거
        Despawn();
    }

    private void Despawn()
    {
        if (_poolKeyPrefab != null && ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.Despawn(_poolKeyPrefab, gameObject);
        else
            gameObject.SetActive(false);
    }
}