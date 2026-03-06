using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageVignetteFlash : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private Volume _volume;

    [Header("Colors")]
    [SerializeField] private Color _shieldColor = new Color(0.2f, 0.6f, 1.0f, 1f);
    [SerializeField] private Color _hpColor = new Color(1.0f, 0.2f, 0.2f, 1f);

    [Header("Flash")]
    [SerializeField] private float _maxIntensity = 0.45f;
    [SerializeField] private float _inTime = 0.06f;
    [SerializeField] private float _outTime = 0.18f;

    private Vignette _vignette;

    private float _prevShield;
    private float _prevHp;

    private bool _shieldHitThisFrame;
    private Coroutine _resetFlagCo;
    private Coroutine _flashCo;

    private void Awake()
    {
        if (_stats == null)
            _stats = FindFirstObjectByType<PlayerStats>();

        if (_volume == null)
            _volume = GetComponent<Volume>();

        if (_volume == null || _volume.profile == null)
        {
            Debug.LogWarning("[VignetteFlash] Volume or Profile missing.");
            return;
        }

        if (!_volume.profile.TryGet(out _vignette) || _vignette == null)
        {
            Debug.LogWarning("[VignetteFlash] Vignette override not found in profile.");
            return;
        }

        _vignette.intensity.Override(0f);
    }

    private void OnEnable()
    {
        if (_stats == null) return;

        _prevShield = _stats.Shield;
        _prevHp = _stats.Hp;

        _stats.OnShieldChanged += OnShieldChanged;
        _stats.OnHpChanged += OnHpChanged;
    }

    private void OnDisable()
    {
        if (_stats == null) return;

        _stats.OnShieldChanged -= OnShieldChanged;
        _stats.OnHpChanged -= OnHpChanged;

        if (_flashCo != null) StopCoroutine(_flashCo);
        _flashCo = null;

        _shieldHitThisFrame = false;
        if (_resetFlagCo != null) StopCoroutine(_resetFlagCo);
        _resetFlagCo = null;

        if (_vignette != null)
            _vignette.intensity.Override(0f);
    }

    private void OnShieldChanged(float cur, float max)
    {
        if (_vignette == null) return;

        if (cur < _prevShield - 0.0001f)
        {
            _shieldHitThisFrame = true;
            EnsureResetFlagEndOfFrame();

            StartFlash(_shieldColor);
        }

        _prevShield = cur;
    }

    private void OnHpChanged(float cur, float max)
    {
        if (_vignette == null) return;

        if (cur < _prevHp - 0.0001f)
        {
            // 같은 프레임에 실드가 먼저 깎였다면 실드 피격
            if (!_shieldHitThisFrame)
                StartFlash(_hpColor);
        }

        _prevHp = cur;
    }

    private void EnsureResetFlagEndOfFrame()
    {
        if (_resetFlagCo != null) return;
        _resetFlagCo = StartCoroutine(CoResetShieldHitFlag());
    }

    private IEnumerator CoResetShieldHitFlag()
    {
        yield return null;
        _shieldHitThisFrame = false;
        _resetFlagCo = null;
    }

    private void StartFlash(Color color)
    {
        if (_flashCo != null)
            StopCoroutine(_flashCo);

        _flashCo = StartCoroutine(CoFlash(color));
    }

    private IEnumerator CoFlash(Color color)
    {
        _vignette.color.Override(color);

        // in
        float t = 0f;
        while (t < _inTime)
        {
            t += Time.unscaledDeltaTime;
            float k = (_inTime <= 0f) ? 1f : Mathf.Clamp01(t / _inTime);
            _vignette.intensity.Override(Mathf.Lerp(0f, _maxIntensity, k));
            yield return null;
        }

        // out
        t = 0f;
        while (t < _outTime)
        {
            t += Time.unscaledDeltaTime;
            float k = (_outTime <= 0f) ? 1f : Mathf.Clamp01(t / _outTime);
            _vignette.intensity.Override(Mathf.Lerp(_maxIntensity, 0f, k));
            yield return null;
        }

        _vignette.intensity.Override(0f);
        _flashCo = null;
    }
}