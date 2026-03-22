using System;
using UnityEngine;
using System.Collections;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float _dayDuration = 90f;
    [SerializeField] private float _nightDuration = 90f;

    [Header("Debug")]
    [SerializeField] private PlayerInputHub _input;
    [SerializeField] private bool _showDebugLog = true;

    public bool IsNight { get; private set; }
    public float CurrentPhaseDuration => IsNight ? _nightDuration : _dayDuration;
    public float PhaseTimeRemaining { get; private set; }
    public float PhaseProgress => 1f - (PhaseTimeRemaining / CurrentPhaseDuration);

    public event Action OnNightTimerEnd;
    public event Action OnDayStart;
    public event Action OnNightStart;

    // 밤 타이머가 끝났지만 아직 낮으로 못 넘어간 상태
    private bool _waitingForCleanup;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (_input == null) _input = FindFirstObjectByType<PlayerInputHub>();
    }

    private void Start()
    {
        BgmManager.Instance?.SubscribeDayNight();

        StartCoroutine(CoDelayedStart());
    }

    private void Update()
    {
        // 디버그 토글
        if (_input != null && _input.ToggleDayNightPressedThisFrame)
        {
            if (IsNight) RequestEnterDay();
            else EnterNight();
            return;
        }

        // Cleanup 대기 중이면 타이머 진행 안 함
        if (_waitingForCleanup) return;

        PhaseTimeRemaining -= Time.deltaTime;

        if (PhaseTimeRemaining <= 0f)
        {
            PhaseTimeRemaining = 0f;

            if (IsNight)
            {
                // 밤 종료, 신호만 발송
                _waitingForCleanup = true;
                OnNightTimerEnd?.Invoke();
            }
            else
            {
                EnterNight();
            }
        }
    }

    public void SetNightDuration(float duration)
    {
        if (duration > 0f)
            _nightDuration = duration;
    }

    public void RequestEnterDay()
    {
        _waitingForCleanup = false;
        EnterDay();
    }

    private void EnterDay()
    {
        IsNight = false;
        PhaseTimeRemaining = _dayDuration;
        OnDayStart?.Invoke();
    }

    private void EnterNight()
    {
        IsNight = true;
        _waitingForCleanup = false;
        PhaseTimeRemaining = _nightDuration;
        OnNightStart?.Invoke();
    }

    private IEnumerator CoDelayedStart()
    {
        yield return null;
        EnterDay();
    }
}