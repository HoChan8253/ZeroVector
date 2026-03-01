using UnityEngine;
using System.Collections;
public class EnemyDeathRotate : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyAI _ai;
    [SerializeField] private Transform _target;

    [Header("Rotation")]
    [SerializeField] private float _zOnDeath = -90f;
    [SerializeField] private bool _useLocalRotation = true;

    [Header("Lock")]
    [SerializeField] private bool _lockWhileDead = true; // 죽은 동안 계속 고정
    private bool _deadStarted;

    private void Awake()
    {
        if (_ai == null) _ai = GetComponentInParent<EnemyAI>();
    }

    private void LateUpdate()
    {
        if (_ai == null || _target == null) return;

        if (_ai.IsDead)
        {
            if (!_deadStarted)
            {
                _deadStarted = true;
                StartCoroutine(CoLockAfterAnimator());
            }
            if (_lockWhileDead)
            {
                Apply();
            }
        }
    }

    private IEnumerator CoLockAfterAnimator()
    {
        yield return new WaitForEndOfFrame();
        Apply();
    }

    private void Apply()
    {
        if (_useLocalRotation)
        {
            var e = _target.localEulerAngles;
            _target.localRotation = Quaternion.Euler(e.x, e.y, _zOnDeath);
        }
        else
        {
            var e = _target.eulerAngles;
            _target.rotation = Quaternion.Euler(e.x, e.y, _zOnDeath);
        }
    }
}