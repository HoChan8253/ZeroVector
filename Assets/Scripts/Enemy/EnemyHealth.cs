using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private bool _stunnable = true;

    [SerializeField] private EnemyData _data;

    private int _hp;
    private EnemyAI _ai;

    private void Awake()
    {
        _hp = _data != null ? _data.maxHp : _maxHp;
        _ai = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int amount, bool headshot)
    {
        if (_hp <= 0) return;

        _hp -= amount;

        bool stun = _stunnable && headshot;
        if (_ai != null) _ai.OnDamaged(transform.position, stun);

        if (_hp <= 0 && _ai != null)
            _ai.Die();
    }
}