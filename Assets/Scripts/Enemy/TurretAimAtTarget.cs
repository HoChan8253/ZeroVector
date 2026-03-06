using UnityEngine;

public class TurretAimAtTarget : MonoBehaviour
{
    [SerializeField] private EnemyAI _ai;
    [SerializeField] private Transform _turret;
    [SerializeField] private float _turnSpeed = 360f;

    private void Awake()
    {
        if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
        if (_turret == null) _turret = transform;
    }

    private void LateUpdate()
    {
        if (_ai == null || _ai.IsDead) return;

        var target = _ai.Target;
        if (target == null) return;

        Vector3 dir = target.position - _turret.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        _turret.rotation = Quaternion.RotateTowards(_turret.rotation, targetRot, _turnSpeed * Time.deltaTime);
    }
}