using UnityEngine;

public class TurretPitchOnlyOnAttack : MonoBehaviour
{
    [SerializeField] private EnemyAI _ai;
    [SerializeField] private Transform _turret;

    [SerializeField] private float _pitchUp = 60f;
    [SerializeField] private float _turnSpeed = 360f;

    private Quaternion _baseLocal;

    private void Awake()
    {
        if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
        if (_turret == null) _turret = transform;

        _baseLocal = _turret.localRotation;
    }

    private void LateUpdate()
    {
        if (_ai == null || _ai.IsDead) return;

        bool attack = _ai.IsAttacking;

        if (!attack)
            _baseLocal = _turret.localRotation;

        Quaternion targetLocal = attack
            ? _baseLocal * Quaternion.Euler(-_pitchUp, 0f, 0f)
            : _baseLocal;

        _turret.localRotation =
            Quaternion.RotateTowards(_turret.localRotation, targetLocal, _turnSpeed * Time.deltaTime);
    }
}