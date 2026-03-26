using UnityEngine;
using System.Collections;

public class ZoneBeacon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ParticleSystem _beaconParticle;
    [SerializeField] private Light _pointLight;

    [Header("Light Pulse")]
    [SerializeField] private float _minIntensity = 1f;
    [SerializeField] private float _maxIntensity = 4f;
    [SerializeField] private float _pulseSpeed = 2f;

    [Header("Fade")]
    [SerializeField] private float _fadeDuration = 5f;

    private Coroutine _pulseCoroutine;

    public void Show(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);

        _beaconParticle?.Play(true);

        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        _pulseCoroutine = StartCoroutine(CoPulse());
    }

    public void Hide()
    {
        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        StartCoroutine(CoFadeAndHide());
    }

    private IEnumerator CoPulse()
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * _pulseSpeed, 1f);
            if (_pointLight != null)
                _pointLight.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, t);
            yield return null;
        }
    }

    private IEnumerator CoFadeAndHide()
    {
        float elapsed = 0f;
        float startIntensity = _pointLight != null ? _pointLight.intensity : 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - elapsed / _fadeDuration;

            if (_pointLight != null)
                _pointLight.intensity = Mathf.Lerp(0f, startIntensity, t);

            yield return null;
        }

        _beaconParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        gameObject.SetActive(false);
    }
}