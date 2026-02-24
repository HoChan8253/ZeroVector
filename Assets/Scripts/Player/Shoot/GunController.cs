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

    [Header("Spread")]
    public float _spreadAmount = 0.02f;   // 기본 퍼짐
    public float _maxSpread = 0.08f;      // 최대 퍼짐
    private float _currentSpread;

    private float _nextFireTime;

    public PlayerLook _look;
    public float _recoilPitchAmount = 2f;
    public float _recoilYawAmount = 1f;

    private void Awake()
    {
        if (_input == null)
            _input = GetComponentInParent<PlayerInputHub>();

        if (_look == null)
            _look = GetComponentInParent<PlayerLook>();
    }

    private void Update()
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

    private void Fire()
    {
        _anim.SetTrigger("Shoot");

        float randomYaw = Random.Range(-_recoilYawAmount, _recoilYawAmount);
        _look.AddRecoil(_recoilPitchAmount, randomYaw);

        _currentSpread += _spreadAmount;
        _currentSpread = Mathf.Clamp(_currentSpread, 0f, _maxSpread);

        Vector3 direction = _cam.transform.forward;
        // 랜덤 탄퍼짐 추가
        direction += _cam.transform.right * Random.Range(-_currentSpread, _currentSpread);
        direction += _cam.transform.up * Random.Range(-_currentSpread, _currentSpread);
        direction.Normalize();

        Ray ray = new Ray(_cam.transform.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, _range))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.2f);

            Health h = hit.collider.GetComponentInParent<Health>();
            if (h != null)
                h.TakeDamage(_damage);
        }
    }
}