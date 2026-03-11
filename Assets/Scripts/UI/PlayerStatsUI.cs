using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private PlayerStatUpgradeManager _upgradeManager;

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

        if (_upgradeManager == null)
            _upgradeManager = FindFirstObjectByType<PlayerStatUpgradeManager>();

        if (_stamina != null) _stamina.minValue = 0f;

        if (_staminaFill != null) _staminaNormalColor = _staminaFill.color;
        if (_hpFill != null) _hpNormalColor = _hpFill.color;
    }

    private void OnEnable()
    {
        if (_stats == null) return;

        _stats.OnHpChanged += OnHpChanged;
        _stats.OnShieldChanged += OnShieldChanged;
        _stats.OnStaminaChanged += OnStaminaChanged;

        // 업그레이드로 최대치가 바뀔 때 전체 갱신
        if (_upgradeManager != null)
            _upgradeManager.OnStatsChanged += ForceRefresh;

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

        if (_upgradeManager != null)
            _upgradeManager.OnStatsChanged -= ForceRefresh;

        StopStaminaBlink();
        StopHpBlink();
    }

    private void ForceRefresh()
    {
        if (_stats == null) return;
        OnHpChanged(_stats.Hp, _stats._maxHp);
        OnShieldChanged(_stats.Shield, _stats._maxShield);
        OnStaminaChanged(_stats.Stamina, _stats._maxStamina);
    }

    private void OnHpChanged(float cur, float max)
    {
        if (_hpAnim != null && max > 0f)
            _hpAnim.Set01(cur / max);

        UpdateHpBlink(cur, max);
    }

    private void OnShieldChanged(float cur, float max)
    {
        if (_shieldAnim != null && max > 0f)
            _shieldAnim.Set01(cur / max);
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
        if (_stats != null && _stats.IsExhausted) StartStaminaBlink();
        else StopStaminaBlink();
    }

    private void StartStaminaBlink()
    {
        if (_staminaBlinkCo != null) return;
        _staminaBlinkCo = StartCoroutine(CoStaminaBlink());
    }

    private void StopStaminaBlink()
    {
        if (_staminaBlinkCo != null) { StopCoroutine(_staminaBlinkCo); _staminaBlinkCo = null; }
        if (_staminaFill != null) _staminaFill.color = _staminaNormalColor;
    }

    private IEnumerator CoStaminaBlink()
    {
        while (_stats != null && _stats.IsExhausted)
        {
            float t = Mathf.PingPong(Time.time * _staminaBlinkSpeed, 1f);
            Color c = _staminaNormalColor * Mathf.Lerp(_staminaMinBrightness, _staminaMaxBrightness, t);
            if (_staminaFill != null) _staminaFill.color = c;
            yield return null;
        }
        StopStaminaBlink();
    }

    // 체력 깜빡임
    private void UpdateHpBlink(float cur, float max)
    {
        if (max <= 0f) { StopHpBlink(); return; }
        if (cur / max <= _hpLowRatio) StartHpBlink();
        else StopHpBlink();
    }

    private void StartHpBlink()
    {
        if (_hpBlinkCo != null) return;
        _hpBlinkCo = StartCoroutine(CoHpBlink());
    }

    private void StopHpBlink()
    {
        if (_hpBlinkCo != null) { StopCoroutine(_hpBlinkCo); _hpBlinkCo = null; }
        if (_hpFill != null) _hpFill.color = _hpNormalColor;
    }

    private IEnumerator CoHpBlink()
    {
        while (_stats != null)
        {
            float max = _stats._maxHp;
            if (max <= 0f) break;
            if (_stats.Hp / max > _hpLowRatio) break;

            float t = Mathf.PingPong(Time.time * _hpBlinkSpeed, 1f);
            Color c = _hpNormalColor * Mathf.Lerp(_hpMinBrightness, _hpMaxBrightness, t);
            if (_hpFill != null) _hpFill.color = c;
            yield return null;
        }
        StopHpBlink();
    }
}