using UnityEngine;
using System.Collections;

public class NightWeatherController : MonoBehaviour
{
    [Header("Skybox")]
    [SerializeField] private Material _daySkybox;
    [SerializeField] private Material _nightSkybox;

    [Header("Directional Light")]
    [SerializeField] private Light _sunLight;
    [SerializeField] private Color _dayLightColor = Color.white;
    [SerializeField] private Color _nightLightColor = new Color(0.1f, 0.15f, 0.3f);
    [SerializeField] private float _dayLightIntensity = 1.2f;
    [SerializeField] private float _nightLightIntensity = 0.2f;

    [Header("Fog")]
    [SerializeField] private Color _dayFogColor = new Color(0.7f, 0.8f, 0.9f);
    [SerializeField] private Color _nightFogColor = new Color(0.01f, 0.02f, 0.05f);
    [SerializeField] private float _dayFogDensity = 0.002f;
    [SerializeField] private float _nightFogDensity = 0.02f;

    [Header("Rain")]
    [SerializeField] private ParticleSystem _rainParticle;

    [Header("Thunder")]
    [SerializeField] private Light _thunderLight;
    [SerializeField] private AudioSource _thunderAudio;
    [SerializeField] private AudioClip[] _thunderClips;
    [SerializeField] private float _thunderMinInterval = 5f;
    [SerializeField] private float _thunderMaxInterval = 15f;

    [Header("Transition")]
    [SerializeField] private float _transitionDuration = 3f;

    private Coroutine _transitionCo;
    private Coroutine _thunderCo;

    private void Start()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnDayStart += OnDayStart;
        DayNightManager.Instance.OnNightStart += OnNightStart;

        ApplyImmediate(isNight: false);
    }

    private void OnDestroy()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnDayStart -= OnDayStart;
        DayNightManager.Instance.OnNightStart -= OnNightStart;
    }

    // 낮
    private void OnDayStart()
    {
        StartTransition(isNight: false);

        // 비 / 천둥 중단
        _rainParticle?.Stop();
        if (_thunderCo != null) { StopCoroutine(_thunderCo); _thunderCo = null; }
        if (_thunderLight != null) _thunderLight.enabled = false;
    }

    // 밤
    private void OnNightStart()
    {
        StartTransition(isNight: true);

        // 비 시작
        _rainParticle?.Play();

        // 천둥 루프 시작
        if (_thunderCo != null) StopCoroutine(_thunderCo);
        _thunderCo = StartCoroutine(CoThunderLoop());
    }

    // 전환
    private void StartTransition(bool isNight)
    {
        if (_transitionCo != null) StopCoroutine(_transitionCo);
        _transitionCo = StartCoroutine(CoTransition(isNight));
    }

    private IEnumerator CoTransition(bool isNight)
    {
        // 스카이박스 즉시 교체
        RenderSettings.skybox = isNight ? _nightSkybox : _daySkybox;
        DynamicGI.UpdateEnvironment();

        Color fromLight = isNight ? _dayLightColor : _nightLightColor;
        Color toLight = isNight ? _nightLightColor : _dayLightColor;
        float fromInt = isNight ? _dayLightIntensity : _nightLightIntensity;
        float toInt = isNight ? _nightLightIntensity : _dayLightIntensity;
        Color fromFog = isNight ? _dayFogColor : _nightFogColor;
        Color toFog = isNight ? _nightFogColor : _dayFogColor;
        float fromDens = isNight ? _dayFogDensity : _nightFogDensity;
        float toDens = isNight ? _nightFogDensity : _dayFogDensity;

        RenderSettings.fog = true;

        float t = 0f;
        while (t < _transitionDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, t / _transitionDuration);

            if (_sunLight != null)
            {
                _sunLight.color = Color.Lerp(fromLight, toLight, s);
                _sunLight.intensity = Mathf.Lerp(fromInt, toInt, s);
            }

            RenderSettings.fogColor = Color.Lerp(fromFog, toFog, s);
            RenderSettings.fogDensity = Mathf.Lerp(fromDens, toDens, s);

            yield return null;
        }

        _transitionCo = null;
    }

    // 번개 루프
    private IEnumerator CoThunderLoop()
    {
        while (true)
        {
            float wait = Random.Range(_thunderMinInterval, _thunderMaxInterval);
            yield return new WaitForSeconds(wait);

            // 천둥 소리
            if (_thunderAudio != null && _thunderClips.Length > 0)
            {
                AudioClip clip = _thunderClips[Random.Range(0, _thunderClips.Length)];
                _thunderAudio.PlayOneShot(clip);
            }

            // 번개 깜빡임
            if (_thunderLight != null)
                StartCoroutine(CoLightningFlash());
        }
    }

    private IEnumerator CoLightningFlash()
    {
        if (_thunderLight == null) yield break;

        // 2~3번 불규칙하게 깜빡임
        int flashes = Random.Range(2, 4);
        for (int i = 0; i < flashes; i++)
        {
            _thunderLight.enabled = true;
            _thunderLight.intensity = Random.Range(2f, 5f);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            _thunderLight.enabled = false;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }
    }

    // 즉시 적용
    private void ApplyImmediate(bool isNight)
    {
        RenderSettings.skybox = isNight ? _nightSkybox : _daySkybox;
        RenderSettings.fog = true;
        RenderSettings.fogColor = isNight ? _nightFogColor : _dayFogColor;
        RenderSettings.fogDensity = isNight ? _nightFogDensity : _dayFogDensity;

        if (_sunLight != null)
        {
            _sunLight.color = isNight ? _nightLightColor : _dayLightColor;
            _sunLight.intensity = isNight ? _nightLightIntensity : _dayLightIntensity;
        }

        DynamicGI.UpdateEnvironment();
    }
}