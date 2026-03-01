using UnityEngine;

public class EnemyTurretAim : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyAI _ai;
    [SerializeField] private Transform _turret;

    [Header("Aim")]
    [SerializeField] private float _turnSpeed = 12f;
    [SerializeField] private bool _onlyWhileAttacking = true;

    [Header("Pitch Clamp")]
    [SerializeField] private float _minPitch = -45f;
    [SerializeField] private float _maxPitch = 90f;

    private void Awake()
    {
        if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
    }

    private void LateUpdate()
    {
        if (_ai == null || _ai.IsDead) return;
        if (_turret == null) return;

        if (_onlyWhileAttacking && !_ai.IsAttacking) return;

        var target = _ai.Target;
        if (target == null) return;

        AimYawPitch(target.position);
    }

    private void AimYawPitch(Vector3 targetPos)
    {
        Vector3 dir = targetPos - _turret.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion lookWorld = Quaternion.LookRotation(dir);

        Transform pivot = _turret.parent;

        if (pivot == null)
        {
            // 월드 회전 자체를 보간
            _turret.rotation = Quaternion.Slerp(_turret.rotation, lookWorld, Time.deltaTime * _turnSpeed);
            return;
        }

        // 월드 목표 회전을 pivot 로컬로 변환
        Quaternion lookLocal = Quaternion.Inverse(pivot.rotation) * lookWorld;
        Vector3 e = lookLocal.eulerAngles;

        float pitch = NormalizeAngle(e.x);
        float yaw = NormalizeAngle(e.y);

        pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);

        Quaternion clampedLocal = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion clampedWorld = pivot.rotation * clampedLocal;

        _turret.rotation = Quaternion.Slerp(_turret.rotation, clampedWorld, Time.deltaTime * _turnSpeed);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}