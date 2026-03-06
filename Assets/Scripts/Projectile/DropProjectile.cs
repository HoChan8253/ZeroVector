using System;
using UnityEngine;

public class DropProjectile : MonoBehaviour
{
    private Vector3 _start;
    private Vector3 _target;
    private float _time;

    private float _t;
    private Action _onImpact;

    public void Init(Vector3 start, Vector3 target, float dropTime, Action onImpact)
    {
        _start = start;
        _target = target;
        _time = Mathf.Max(0.01f, dropTime);
        _onImpact = onImpact;

        _t = 0f;
        transform.position = _start;

        Vector3 dir = (_target - _start);
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private void Update()
    {
        float prev = Mathf.Clamp01(_t);
        _t += Time.deltaTime / _time;
        float tt = Mathf.Clamp01(_t);

        Vector3 p0 = Vector3.Lerp(_start, _target, prev);
        Vector3 p1 = Vector3.Lerp(_start, _target, tt);

        transform.position = p1;

        // 떨어질 때 Z가 아래
        Vector3 v = (p1 - p0);
        if (v.sqrMagnitude > 0.0000001f)
            transform.rotation = Quaternion.LookRotation(v.normalized);

        if (tt >= 1f)
        {
            _onImpact?.Invoke();
            Destroy(gameObject);
        }
    }
}