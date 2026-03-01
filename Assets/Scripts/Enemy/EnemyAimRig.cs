using UnityEngine;

public class EnemyAimRig : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyAI _ai;
    [SerializeField] private Transform _head; // 머리 (회전축)
    [SerializeField] private Transform _base; // 바디/바퀴 (회전축)

    [Header("Head Aim")]
    [SerializeField] private float _headTurnSpeed = 12f;

    [SerializeField] private float _minPitch = -45f;
    [SerializeField] private float _maxPitch = 90f;

    [Header("Base Turn")]
    [SerializeField] private float _baseTurnSpeed = 5f;
    [SerializeField] private bool _baseFaceTargetOnlyWhenMoving = true; // 이동할 때만 바디 회전

    private void Awake()
    {
        if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
        if (_base == null) _base = transform;
    }

    private void LateUpdate()
    {
        if (_ai == null || _ai.IsDead) return;

        var target = _ai.Target;
        if (target == null) return;

        UpdateHead(target.position);
        UpdateBase(target.position);
    }

    private void UpdateHead(Vector3 targetPos)
    {
        if (_head == null) return;

        // 월드 기준 방향
        Vector3 dir = targetPos - _head.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        // 머리 회전 목표
        Quaternion look = Quaternion.LookRotation(dir);

        // 로컬 피치 제한을 위해 base 기준 로컬로 변환
        Transform pivot = _base != null ? _base : _head.parent;
        if (pivot == null)
        {
            // pivot이 없으면 그냥 회전
            _head.rotation = Quaternion.Slerp(_head.rotation, look, Time.deltaTime * _headTurnSpeed);
            return;
        }

        Quaternion local = Quaternion.Inverse(pivot.rotation) * look;
        Vector3 e = local.eulerAngles;

        // eulerAngle을 -180~180으로 정규화
        float pitch = NormalizeAngle(e.x);
        float yaw = NormalizeAngle(e.y);

        // 위로 90도 제한
        pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);

        // 롤은 0으로 고정
        Quaternion clampedLocal = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion clampedWorld = pivot.rotation * clampedLocal;

        _head.rotation = Quaternion.Slerp(_head.rotation, clampedWorld, Time.deltaTime * _headTurnSpeed);
    }

    private void UpdateBase(Vector3 targetPos)
    {
        if (_base == null) return;

        if (_baseFaceTargetOnlyWhenMoving)
        {
            // 이동 중일 때만 바디가 타겟 방향
            if (!_ai.IsMoving) return;

            // 공격 중에는 바디 고정
            if (_ai.IsAttacking && _ai.LastAttackVariant != 1) return;
        }

        Vector3 dir = targetPos - _base.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        _base.rotation = Quaternion.Slerp(_base.rotation, look, Time.deltaTime * _baseTurnSpeed);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}