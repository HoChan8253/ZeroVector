using UnityEngine;

public class GunHitscan : MonoBehaviour
{
    [Header("Refs")]
    public Camera _cam;
    public PlayerInputHub _input;

    [Header("Gun")]
    public float _range = 100f;
    public float _damage = 25f;
    public float _fireCooldown = 0.12f;

    private float _nextFireTime;

    void Awake()
    {
        if (_input == null) _input = GetComponent<PlayerInputHub>();
    }

    void Update()
    {
        if (_input.FireHeld && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireCooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _range))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
                h.TakeDamage(_damage);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * _range, Color.white, 0.1f);
        }
    }
}
