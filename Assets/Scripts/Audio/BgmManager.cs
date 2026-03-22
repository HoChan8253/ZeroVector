using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance { get; private set; }

    [Header("BGM Clip")]
    [SerializeField] private AudioClip[] _dayBgms;
    [SerializeField] private AudioClip[] _nightBgms;

    [Header("Audio")]
    [SerializeField] private AudioMixerGroup _bgmMixerGroup;
    [SerializeField] private AudioSource _audioSource;

    [Header("Fade")]
    [SerializeField] private float _fadeDuration = 1.5f;

    private Coroutine _playCoroutine;
    private int _lastIndex = -1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_bgmMixerGroup != null)
            _audioSource.outputAudioMixerGroup = _bgmMixerGroup;
    }

    private void Start()
    {
        SubscribeDayNight();
    }

    private void OnEnable()
    {
        SubscribeDayNight();
    }

    private void OnDestroy()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnDayStart -= OnDayStart;
        DayNightManager.Instance.OnNightStart -= OnNightStart;
    }

    public void SubscribeDayNight()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnDayStart -= OnDayStart;
        DayNightManager.Instance.OnNightStart -= OnNightStart;
        DayNightManager.Instance.OnDayStart += OnDayStart;
        DayNightManager.Instance.OnNightStart += OnNightStart;

        if (!DayNightManager.Instance.IsNight)
            OnDayStart();
        else
            OnNightStart();
    }

    private void OnDayStart()
    {
        _lastIndex = -1;
        PlayBgmList(_dayBgms);
    }

    private void OnNightStart()
    {
        _lastIndex = -1;
        PlayBgmList(_nightBgms);
    }

    private void PlayBgmList(AudioClip[] clips)
    {
        if (_playCoroutine != null) StopCoroutine(_playCoroutine);
        _playCoroutine = StartCoroutine(CoPlayLoop(clips));
    }

    private IEnumerator CoPlayLoop(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) yield break;

        // 현재 재생 중이면 페이드 아웃
        if (_audioSource.isPlaying)
            yield return StartCoroutine(CoFade(0f));

        while (true)
        {
            // 랜덤 선택 (이전 곡 연속 방지)
            int index;
            if (clips.Length == 1)
            {
                index = 0;
            }
            else
            {
                do { index = Random.Range(0, clips.Length); }
                while (index == _lastIndex);
            }
            _lastIndex = index;

            _audioSource.clip = clips[index];
            _audioSource.Play();

            // 페이드 인
            yield return StartCoroutine(CoFade(1f));

            // 곡이 끝나기 fadeDuration 전에 페이드 아웃 시작
            float waitTime = _audioSource.clip.length - _fadeDuration;
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            // 페이드 아웃
            yield return StartCoroutine(CoFade(0f));

            _audioSource.Stop();

            // 다음 곡 전 짧은 딜레이
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator CoFade(float targetVolume)
    {
        float startVolume = _audioSource.volume;
        float t = 0f;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t / _fadeDuration);
            yield return null;
        }
        _audioSource.volume = targetVolume;
    }

    public void Stop()
    {
        if (_playCoroutine != null)
        {
            StopCoroutine(_playCoroutine);
            _playCoroutine = null;
        }
        _audioSource.Stop();
        _audioSource.volume = 1f;
    }
}