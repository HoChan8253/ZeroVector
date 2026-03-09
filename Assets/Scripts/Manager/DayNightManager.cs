using System;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float _dayDuration = 90f;    // 낮 지속 시간
    [SerializeField] private float _nightDuration = 90f;  // 밤 지속 시간

    [Header("Debug")]
    [SerializeField] private PlayerInputHub _input;       // 테스트용 토글
    [SerializeField] private bool _showDebugLog = true;

    public bool IsNight { get; private set; }
    public float CurrentPhaseDuration => IsNight ? _nightDuration : _dayDuration;
    public float PhaseTimeRemaining { get; private set; }  // 현재 페이즈 남은 시간
    public float PhaseProgress => 1f - (PhaseTimeRemaining / CurrentPhaseDuration); // 0~1

    public event Action OnDayStart;
    public event Action OnNightStart;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (_input == null) _input = FindFirstObjectByType<PlayerInputHub>();
    }

    private void Start()
    {
        EnterDay();
    }

    private void Update()
    {
        if (_input != null && _input.ToggleDayNightPressedThisFrame)
        {
            if (IsNight) EnterDay();
            else EnterNight();
            return;
        }

        // 타이머
        PhaseTimeRemaining -= Time.deltaTime;
        if (PhaseTimeRemaining <= 0f)
        {
            if (IsNight) EnterDay();
            else EnterNight();
        }
    }

    private void EnterDay()
    {
        IsNight = false;
        PhaseTimeRemaining = _dayDuration;
        if (_showDebugLog) Debug.Log($"[DayNight] 낮 시작 - {_dayDuration}초");
        OnDayStart?.Invoke();
    }

    private void EnterNight()
    {
        IsNight = true;
        PhaseTimeRemaining = _nightDuration;
        if (_showDebugLog) Debug.Log($"[DayNight] 밤 시작 - {_nightDuration}초");
        OnNightStart?.Invoke();
    }
}