using UnityEngine;
using System.Collections;

public class BossHealthUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyHealth _bossHealth;

    [Header("HP Animation")]
    [SerializeField] private StatsBarAnim _hpAnim;

    private void Start()
    {
        if (_bossHealth == null)
            _bossHealth = FindFirstObjectByType<BossAI>()?.GetComponent<EnemyHealth>();

        ForceRefresh();
    }

    private void OnEnable()
    {
        if (_bossHealth == null) return;
        _bossHealth.OnHpChanged += OnHpChanged;
        _bossHealth.OnDead += OnDead;
        ForceRefresh();
    }

    private void OnDisable()
    {
        if (_bossHealth == null) return;
        _bossHealth.OnHpChanged -= OnHpChanged;
        _bossHealth.OnDead -= OnDead;
    }

    private void OnHpChanged(int cur, int max)
    {
        if (_hpAnim == null || max <= 0) return;
        _hpAnim.Set01((float)cur / max);
    }

    private void ForceRefresh()
    {
        if (_bossHealth == null || _hpAnim == null) return;
        _hpAnim.Set01((float)_bossHealth.Hp / _bossHealth.MaxHp);
    }

    private void OnDead()
    {
        _hpAnim?.Set01(0f);
    }

    public void Bind(EnemyHealth health)
    {
        if (_bossHealth != null)
        {
            _bossHealth.OnHpChanged -= OnHpChanged;
            _bossHealth.OnDead -= OnDead;
        }
        _bossHealth = health;
        gameObject.SetActive(true);
    }
}