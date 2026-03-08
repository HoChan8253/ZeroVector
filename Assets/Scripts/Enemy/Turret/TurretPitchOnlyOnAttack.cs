using UnityEngine;

public class TurretPitchOnlyOnAttack : MonoBehaviour
{
    [SerializeField] private EnemyAI _ai;
    [SerializeField] private Transform _turret;

    [SerializeField] private float _pitchUp = 60f;
    [SerializeField] private float _turnSpeed = 360f;

    private Quaternion _baseLocal;
    private bool _wasAttacking;

    private void Awake()
    {
        if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
        if (_turret == null) _turret = transform;

        _baseLocal = _turret.localRotation;
        _wasAttacking = false;
    }

    private void LateUpdate()
    {
        if (_ai == null || _ai.IsDead) return;

        bool attacking = _ai.IsAttacking;

        // 공격 시작 순간에만 현재 각도를 원래 위치로 캡처
        if (attacking && !_wasAttacking)
            _baseLocal = _turret.localRotation;

        // 공격 중 위를 봄 / 공격 아닐 때 원래 위치로 복귀
        Quaternion targetLocal =
            attacking
            ? _baseLocal * Quaternion.Euler(-_pitchUp, 0f, 0f)
            : _baseLocal;

        _turret.localRotation =
            Quaternion.RotateTowards(_turret.localRotation, targetLocal, _turnSpeed * Time.deltaTime);

        _wasAttacking = attacking;
    }
}