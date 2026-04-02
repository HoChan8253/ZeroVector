using System;
using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float _maxHp = 100f;
    public float Hp { get; private set; }

    [Header("Shield")]
    public float _maxShield = 50f;
    public float Shield { get; private set; }
    public float _shieldRegenDelay = 6.0f;
    public float _shieldRegenPerSec = 4.0f;

    [Header("Stamina")]
    public float _maxStamina = 120f;
    public float Stamina { get; private set; }
    public float _staminaDrainPerSec = 20f;     // 초당 소모
    public float _staminaRegenPerSec = 10f;     // 초당 회복
    public float _exhaustRecoverThreshold = 40f; // 여기까지 차야 스프린트 재허용

    public bool IsReloading { get; private set; }

    public bool IsDead => Hp <= 0f;

    // 탈진 상태: 0이 되면 true, threshold까지 회복되면 false
    public bool IsExhausted { get; private set; }
    public bool CanSprint => !IsDead && !IsReloading && !IsExhausted && Stamina > 0.05f;

    public event Action<float, float> OnHpChanged;
    public event Action<float, float> OnShieldChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnDie;

    private float _shieldRegenResumeTime;

    private float _staminaBoostMultiplier = 1f;
    private Coroutine _staminaBoostCo;

    private bool _isInvincible = false;

    private void Awake()
    {
        Hp = _maxHp;
        Shield = _maxShield;
        Stamina = _maxStamina;
        IsExhausted = false;

        OnHpChanged?.Invoke(Hp, _maxHp);
        OnShieldChanged?.Invoke(Shield, _maxShield);
        OnStaminaChanged?.Invoke(Stamina, _maxStamina);
    }

    private void Update()
    {
        RegenShield();
    }

    private void RegenShield()
    {
        if (Shield >= _maxShield) return;
        if (Time.time < _shieldRegenResumeTime) return;

        Shield += _shieldRegenPerSec * Time.deltaTime;
        if (Shield > _maxShield) Shield = _maxShield;

        OnShieldChanged?.Invoke(Shield, _maxShield);
    }

    public void SetReloading(bool isReloading)
    {
        IsReloading = isReloading;
    }

    public void TickStamina(bool isSprinting)
    {
        float prev = Stamina;

        if (isSprinting)
        {
            Stamina -= _staminaDrainPerSec * Time.deltaTime;
            if (Stamina <= 0f)
            {
                Stamina = 0f;
                IsExhausted = true; // 탈진
            }
        }
        else
        {
            Stamina += _staminaRegenPerSec * _staminaBoostMultiplier * Time.deltaTime;
            if (Stamina > _maxStamina) Stamina = _maxStamina;

            // 탈진이면 스테미너 회복이 일정 수준까지 회복해야 스프린트 잠금 해제
            if (IsExhausted && Stamina >= _exhaustRecoverThreshold)
                IsExhausted = false;
        }

        if (!Mathf.Approximately(prev, Stamina))
            OnStaminaChanged?.Invoke(Stamina, _maxStamina);
    }

    public void TakeDamage(int amount)
    {
        TakeDamage((float)amount);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        if (_isInvincible) return;
        if (damage <= 0f) return;

        _shieldRegenResumeTime = Time.time + _shieldRegenDelay;

        float remain = damage;

        if (Shield > 0f)
        {
            float used = Mathf.Min(Shield, remain);
            Shield -= used;
            remain -= used;
            OnShieldChanged?.Invoke(Shield, _maxShield);
        }

        if (remain > 0f)
        {
            Hp -= remain;
            if (Hp < 0f) Hp = 0f;
            OnHpChanged?.Invoke(Hp, _maxHp);

            if (Hp <= 0f)
                OnDie?.Invoke();
        }
    }

    //최대치를 초과하지 않는 선에서 HP 회복
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        Hp = Mathf.Min(Hp + amount, _maxHp);
        OnHpChanged?.Invoke(Hp, _maxHp);
    }

    //최대치를 초과하지 않는 선에서 Shield 회복
    public void HealShield(float amount)
    {
        if (amount <= 0f) return;
        Shield = Mathf.Min(Shield + amount, _maxShield);
        OnShieldChanged?.Invoke(Shield, _maxShield);
    }

    public void ConsumeStamina(float amount)
    {
        if (amount <= 0f) return;
        Stamina -= amount;
        if (Stamina <= 0f)
        {
            Stamina = 0f;
            IsExhausted = true;
        }
        OnStaminaChanged?.Invoke(Stamina, _maxStamina);
    }

    public void ApplyStaminaBoost(float multiplier, float duration)
    {
        if (_staminaBoostCo != null) StopCoroutine(_staminaBoostCo);
        _staminaBoostCo = StartCoroutine(CoStaminaBoost(multiplier, duration));
    }

    public void SetInvincible(bool value)
    {
        _isInvincible = value;
    }

    private IEnumerator CoStaminaBoost(float multiplier, float duration)
    {
        _staminaBoostMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        _staminaBoostMultiplier = 1f;
    }
}