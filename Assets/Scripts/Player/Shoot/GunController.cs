using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Refs")]
    public Camera _cam;
    public Animator _anim;
    public PlayerInputHub _input;

    [Header("Gun Settings")]
    public float _fireRate = 10f;
    public float _range = 100f;
    public float _damage = 25f;

    private float _nextFireTime;

    public PlayerLook _look;
    public float _recoilPitchAmount = 2f;
    public float _recoilYawAmount = 1f;

    void Awake()
    {
        if (_input == null)
            _input = GetComponentInParent<PlayerInputHub>();

        if (_look == null)
            _look = GetComponentInParent<PlayerLook>();
    }

    void Update()
    {
        // 연사 조건
        if (_input.FireHeld && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + (1f / _fireRate);
            Fire();
        }

        if (_input.ReloadPressedThisFrame)
        {
            _anim.SetTrigger("Reload");
        }
    }

    void Fire()
    {
        _anim.SetTrigger("Shoot");

        float randomYaw = Random.Range(-_recoilYawAmount, _recoilYawAmount);
        _look.AddRecoil(_recoilPitchAmount, randomYaw);

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _range))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
                h.TakeDamage(_damage);
        }
    }
}