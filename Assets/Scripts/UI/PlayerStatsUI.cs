using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats _stats;

    [Header("Sliders")]
    [SerializeField] private Slider _stamina;

    [Header("HP Animation")]
    [SerializeField] private StatsBarAnim _hpAnim;
    [SerializeField] private StatsBarAnim _shieldAnim;

    [Header("Stamina Blink (Exhausted)")]
    [SerializeField] private Image _staminaFill;
    [SerializeField] private float _staminaBlinkSpeed = 20f;
    [SerializeField] private float _staminaMinBrightness = 0.3f;
    [SerializeField] private float _staminaMaxBrightness = 1.4f;

    [Header("HP Blink")]
    [SerializeField] private Image _hpFill;
    [SerializeField] private float _hpBlinkSpeed = 10f;
    [SerializeField] private float _hpLowRatio = 0.2f;
    [SerializeField] private float _hpMinBrightness = 0.3f;
    [SerializeField] private float _hpMaxBrightness = 1.4f;

    private Coroutine _staminaBlinkCo;
    private Coroutine _hpBlinkCo;

    private Color _staminaNormalColor;
    private Color _hpNormalColor;

    private void Awake()
    {
        if (_stats == null)
            _stats = FindFirstObjectByType<PlayerStats>();

        if (_stamina != null) _stamina.minValue = 0f;

        if (_staminaFill != null)
            _staminaNormalColor = _staminaFill.color;

        if (_hpFill != null)
            _hpNormalColor = _hpFill.color;
    }

    private void OnEnable()
    {
        if (_stats == null) return;

        _stats.OnHpChanged += OnHpChanged;
        _stats.OnShieldChanged += OnShieldChanged;
        _stats.OnStaminaChanged += OnStaminaChanged;

        ForceRefresh();
        UpdateStaminaBlink();
        UpdateHpBlink(_stats.Hp, _stats._maxHp);
    }

    private void OnDisable()
    {
        if (_stats == null) return;

        _stats.OnHpChanged -= OnHpChanged;
        _stats.OnShieldChanged -= OnShieldChanged;
        _stats.OnStaminaChanged -= OnStaminaChanged;

        StopStaminaBlink();
        StopHpBlink();
    }

    private void ForceRefresh()
    {
        OnHpChanged(_stats.Hp, _stats._maxHp);
        OnShieldChanged(_stats.Shield, _stats._maxShield);
        OnStaminaChanged(_stats.Stamina, _stats._maxStamina);
    }

    private void OnHpChanged(float cur, float max)
    {
        if (_hpAnim != null && max > 0f)
        {
            float ratio01 = cur / max;
            _hpAnim.Set01(ratio01);
        }

        UpdateHpBlink(cur, max);
    }

    private void OnShieldChanged(float cur, float max)
    {
        if (_shieldAnim != null && max > 0f)
        {
            float ratio01 = cur / max;
            _shieldAnim.Set01(ratio01);
        }
    }

    private void OnStaminaChanged(float cur, float max)
    {
        if (_stamina != null)
        {
            _stamina.maxValue = max;
            _stamina.value = cur;
        }

        UpdateStaminaBlink();
    }

    // 스테미너 깜빡임
    private void UpdateStaminaBlink()
    {
        if (_stats != null && _stats.IsExhausted)
            StartStaminaBlink();
        else
            StopStaminaBlink();
    }

    private void StartStaminaBlink()
    {
        if (_staminaBlinkCo != null) return;
        _staminaBlinkCo = StartCoroutine(CoStaminaBlink());
    }

    private void StopStaminaBlink()
    {
        if (_staminaBlinkCo != null)
        {
            StopCoroutine(_staminaBlinkCo);
            _staminaBlinkCo = null;
        }

        if (_staminaFill != null)
            _staminaFill.color = _staminaNormalColor;
    }

    private IEnumerator CoStaminaBlink()
    {
        while (_stats != null && _stats.IsExhausted)
        {
            float t = Mathf.PingPong(Time.time * _staminaBlinkSpeed, 1f);

            Color c = _staminaNormalColor;
            c *= Mathf.Lerp(_staminaMinBrightness, _staminaMaxBrightness, t);

            if (_staminaFill != null)
                _staminaFill.color = c;

            yield return null;
        }

        StopStaminaBlink();
    }

    // 체력 깜빡임
    private void UpdateHpBlink(float cur, float max)
    {
        if (max <= 0f) { StopHpBlink(); return; }

        float ratio = cur / max;
        if (ratio <= _hpLowRatio)
            StartHpBlink();
        else
            StopHpBlink();
    }

    private void StartHpBlink()
    {
        if (_hpBlinkCo != null) return;
        _hpBlinkCo = StartCoroutine(CoHpBlink());
    }

    private void StopHpBlink()
    {
        if (_hpBlinkCo != null)
        {
            StopCoroutine(_hpBlinkCo);
            _hpBlinkCo = null;
        }

        if (_hpFill != null)
            _hpFill.color = _hpNormalColor;
    }

    private IEnumerator CoHpBlink()
    {
        while (_stats != null)
        {
            float max = _stats._maxHp;
            if (max <= 0f) break;

            float ratio = _stats.Hp / max;
            if (ratio > _hpLowRatio) break;

            float t = Mathf.PingPong(Time.time * _hpBlinkSpeed, 1f);

            Color c = _hpNormalColor;
            c *= Mathf.Lerp(_hpMinBrightness, _hpMaxBrightness, t);

            if (_hpFill != null)
                _hpFill.color = c;

            yield return null;
        }

        StopHpBlink();
    }
}