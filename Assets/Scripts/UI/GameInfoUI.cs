using TMPro;
using UnityEngine;
using DG.Tweening;

public class GameInfoUI : MonoBehaviour
{
    [Header("타이머")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _phaseText;

    [Header("웨이브")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemyCountText;
    [SerializeField] private GameObject _enemyCountPanel;

    [Header("색상")]
    [SerializeField] private Color _dayColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color _nightColor = new Color(0.4f, 0.7f, 1f);
    [SerializeField] private Color _warningColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color _cleanupColor = new Color(1f, 0.5f, 0f); // 주황

    [Header("경고 시간")]
    [SerializeField] private float _warningThreshold = 10f;

    private WaveManager _wave;
    private bool _isWarning;
    private bool _isCleanupPhase;

    private void Start()
    {
        _wave = FindFirstObjectByType<WaveManager>();

        var dnm = DayNightManager.Instance;
        if (dnm != null)
        {
            dnm.OnDayStart += HandleDayStart;
            dnm.OnNightStart += HandleNightStart;
            dnm.OnNightTimerEnd += HandleNightTimerEnd;
        }

        if (_wave != null)
        {
            _wave.OnWaveStart += HandleWaveStart;
            _wave.OnWaveEnd += HandleWaveEnd;
        }

        SetPhase(false);
        HideEnemyCount();
    }

    private void OnDestroy()
    {
        var dnm = DayNightManager.Instance;
        if (dnm != null)
        {
            dnm.OnDayStart -= HandleDayStart;
            dnm.OnNightStart -= HandleNightStart;
            dnm.OnNightTimerEnd -= HandleNightTimerEnd;
        }

        if (_wave != null)
        {
            _wave.OnWaveStart -= HandleWaveStart;
            _wave.OnWaveEnd -= HandleWaveEnd;
        }
    }

    private void Update()
    {
        UpdateTimer();

        if (_isCleanupPhase)
            UpdateEnemyCount();
    }

    // 타이머
    private void UpdateTimer()
    {
        if (DayNightManager.Instance == null) return;

        if (DayNightManager.Instance.IsInfiniteNight)
        {
            _timerText.text = "";
            return;
        }

        if (_isCleanupPhase)
        {
            _timerText.text = "";
            return;
        }

        float remaining = DayNightManager.Instance.PhaseTimeRemaining;
        int min = Mathf.FloorToInt(remaining / 60f);
        int sec = Mathf.FloorToInt(remaining % 60f);
        _timerText.text = min > 0 ? $"{min}:{sec:00}" : $"{sec}";

        bool warning = remaining <= _warningThreshold && remaining > 0f;
        if (warning != _isWarning)
        {
            _isWarning = warning;
            bool isNight = DayNightManager.Instance.IsNight;
            Color target = _isWarning ? _warningColor
                         : isNight ? _nightColor
                                      : _dayColor;
            _timerText.DOColor(target, 0.3f);
            if (_isWarning)
                _timerText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 5, 0.5f);
        }
    }

    private void UpdateEnemyCount()
    {
        if (_wave == null) return;
        _enemyCountText.text = $"남은 적 : {_wave.AliveEnemyCount}";
    }

    // 페이즈 전환
    private void HandleDayStart()
    {
        _isCleanupPhase = false;
        SetPhase(false);
        HideEnemyCount();
    }

    private void HandleNightStart()
    {
        _isCleanupPhase = false;
        SetPhase(true);
        HideEnemyCount();
    }

    private void HandleNightTimerEnd()
    {
        _isCleanupPhase = true;
        _isWarning = false;

        if (_phaseText != null)
        {
            _phaseText.text = "정리";
            _phaseText.DOColor(_cleanupColor, 0.5f);
        }
        _timerText.DOColor(_cleanupColor, 0.3f);

        ShowEnemyCount();
    }

    private void SetPhase(bool isNight)
    {
        _isWarning = false;
        if (_phaseText != null)
        {
            _phaseText.text = isNight ? "밤" : "낮";
            _phaseText.DOColor(isNight ? _nightColor : _dayColor, 0.5f);
        }
        _timerText.DOColor(isNight ? _nightColor : _dayColor, 0.5f);
    }

    // 웨이브
    private void HandleWaveStart(int wave)
    {
        if (_waveText == null) return;
        _waveText.text = $"Wave {wave}";
        _waveText.transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 5, 0.5f);
    }

    private void HandleWaveEnd(int wave)
    {
        HideEnemyCount();
        _isCleanupPhase = false;
    }

    // 잔여 적 패널
    private void ShowEnemyCount()
    {
        if (_enemyCountPanel == null) return;
        _enemyCountPanel.SetActive(true);
        _enemyCountPanel.transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 5, 0.5f);
    }

    private void HideEnemyCount()
    {
        if (_enemyCountPanel == null) return;
        _enemyCountPanel.SetActive(false);
    }
}