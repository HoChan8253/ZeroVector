using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 장판 + 지속딜
public class AoEZone : MonoBehaviour
{
    [SerializeField] private SphereCollider _col;

    private readonly List<IDamageable> _targets = new();
    private int _tickDamage;
    private float _tickInterval;
    private float _endTime;

    public void Init(float radius, int tickDamage, float tickInterval, float lifeTime)
    {
        if (_col == null) _col = GetComponent<SphereCollider>();

        _tickDamage = tickDamage;
        _tickInterval = tickInterval;
        _endTime = Time.time + lifeTime;

        _targets.Clear();

        _col.isTrigger = true;
        _col.radius = radius;

        StartCoroutine(CoTick());
    }

    private IEnumerator CoTick()
    {
        while (Time.time < _endTime)
        {
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                var t = _targets[i];
                if (t == null) { _targets.RemoveAt(i); continue; }
                t.TakeDamage(_tickDamage);
            }

            yield return new WaitForSeconds(_tickInterval);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        var d = other.GetComponentInParent<IDamageable>();
        if (d != null && !_targets.Contains(d))
            _targets.Add(d);
    }

    private void OnTriggerExit(Collider other)
    {
        var d = other.GetComponentInParent<IDamageable>();
        if (d != null)
            _targets.Remove(d);
    }
}