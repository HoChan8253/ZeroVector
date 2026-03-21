using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Wave Data")]
    [SerializeField] private WaveData[] _waveDatas;

    [Header("Wave Settings")]
    [SerializeField] private int _totalWaves = 10;

    [Header("Auto Scaling")]
    [Tooltip("WaveData가 없는 웨이브에 사용할 기본 적 프리팹")]
    [SerializeField] private GameObject[] _fallbackEnemyPool;
    [SerializeField] private int _fallbackBaseCount = 10;
    [SerializeField] private int _fallbackCountPerWave = 2;
    [SerializeField] private float _fallbackSpawnInterval = 0.4f;

    [Header("Clear Bonus")]
    [SerializeField] private int _baseClearBonus = 300;
    [SerializeField] private int _bonusPerWave = 200;
    [SerializeField] private Transform _player;
    [SerializeField] private WaveClearBonusUI _clearBonusUI;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = true;

    public int CurrentWave { get; private set; }
    public bool IsWaveActive { get; private set; }
    public int AliveEnemyCount => _aliveEnemies.Count;

    public event Action<int> OnWaveStart;
    public event Action<int> OnWaveEnd;
    public event Action OnAllWavesCleared;
    public event Action OnCleanupPhaseStart;

    private enum Phase { Idle, Day, Night, Cleanup }
    private Phase _phase = Phase.Idle;

    private readonly List<GameObject> _aliveEnemies = new();
    private Coroutine _fieldLoopCoroutine;

    private void Start()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnDayStart += HandleDayStart;
        DayNightManager.Instance.OnNightStart += HandleNightStart;
        DayNightManager.Instance.OnNightTimerEnd += HandleNightTimerEnd;
    }

    private void OnDestroy()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnDayStart -= HandleDayStart;
        DayNightManager.Instance.OnNightStart -= HandleNightStart;
        DayNightManager.Instance.OnNightTimerEnd -= HandleNightTimerEnd;
    }

    private void HandleDayStart()
    {
        _phase = Phase.Day;
        CurrentWave++;

        WaveData data = GetWaveData(CurrentWave);
        if (data != null && data.nightDuration > 0f)
            DayNightManager.Instance.SetNightDuration(data.nightDuration);

        OnWaveStart?.Invoke(CurrentWave);
        StartFieldLoop(CoDayFieldLoop());
    }

    private IEnumerator CoDayFieldLoop()
    {
        WaveData data = GetWaveData(CurrentWave);
        int target = GetDayTarget(data);
        float interval = data != null ? data.spawnInterval : _fallbackSpawnInterval;

        while (_phase == Phase.Day)
        {
            if (_aliveEnemies.Count < target)
            {
                GameObject prefab = PickOne(data, isNight: false);
                if (prefab != null) SpawnEnemy(prefab);
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void HandleNightStart()
    {
        _phase = Phase.Night;
        IsWaveActive = true;

        ActivateAllEnemies();
        StartFieldLoop(CoNightRegenLoop());
    }

    private IEnumerator CoNightRegenLoop()
    {
        WaveData data = GetWaveData(CurrentWave);
        int target = GetDayTarget(data);
        float interval = data != null ? data.spawnInterval : _fallbackSpawnInterval;

        while (_phase == Phase.Night)
        {
            if (_aliveEnemies.Count < target)
            {
                GameObject prefab = PickOne(data, isNight: true);
                if (prefab != null) SpawnEnemy(prefab);
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void HandleNightTimerEnd()
    {
        _phase = Phase.Cleanup;
        StopFieldLoop();

        OnCleanupPhaseStart?.Invoke();

        if (_aliveEnemies.Count == 0)
            FinishWave();
    }

    private void OnEnemyDead(GameObject enemy)
    {
        _aliveEnemies.Remove(enemy);
        if (_showDebugLog) Debug.Log($"[Wave] 적 처치 - 남은 수: {_aliveEnemies.Count} / Phase: {_phase}");

        if (_phase == Phase.Cleanup && _aliveEnemies.Count == 0)
            FinishWave();
    }

    private void FinishWave()
    {
        IsWaveActive = false;

        GiveClearBonus();

        OnWaveEnd?.Invoke(CurrentWave);

        if (CurrentWave >= _totalWaves)
        {
            OnAllWavesCleared?.Invoke();
            return;
        }

        DayNightManager.Instance?.RequestEnterDay();
    }

    private void GiveClearBonus()
    {
        if (GoldManager.Instance == null) return;

        int bonus = _baseClearBonus + _bonusPerWave * CurrentWave;
        Vector3 pos = _player != null ? _player.position : Vector3.zero;
        GoldManager.Instance.Add(bonus, pos);

        if (_clearBonusUI != null)
            _clearBonusUI.Show(CurrentWave, bonus);
    }

    // 루프 관리
    private void StartFieldLoop(IEnumerator loop)
    {
        StopFieldLoop();
        _fieldLoopCoroutine = StartCoroutine(loop);
    }

    private void StopFieldLoop()
    {
        if (_fieldLoopCoroutine == null) return;
        StopCoroutine(_fieldLoopCoroutine);
        _fieldLoopCoroutine = null;
    }

    // 스폰 유틸
    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || _spawnPoints.Length == 0) return;

        Transform point = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
        var go = Instantiate(prefab, point.position, point.rotation);
        _aliveEnemies.Add(go);

        if (go.TryGetComponent<EnemyHealth>(out var health))
            health.OnDead += () => OnEnemyDead(go);
    }

    private void ActivateAllEnemies()
    {
        foreach (var go in _aliveEnemies)
        {
            if (go == null) continue;
            if (go.TryGetComponent<IEnemyAI>(out var ai))
                ai.ActivateCombat();
        }
    }

    // 유틸
    private int GetDayTarget(WaveData data)
    {
        return data != null ? data.dayFieldTarget
                            : _fallbackBaseCount + _fallbackCountPerWave * (CurrentWave - 1);
    }

    private GameObject PickOne(WaveData data, bool isNight)
    {
        if (data != null)
        {
            var entries = isNight ? data.nightEntries : data.dayEntries;
            var picked = data.PickPrefabs(entries, 1);
            if (picked.Length > 0 && picked[0] != null) return picked[0];
        }

        if (_fallbackEnemyPool.Length == 0) return null;
        return _fallbackEnemyPool[UnityEngine.Random.Range(0, _fallbackEnemyPool.Length)];
    }

    private WaveData GetWaveData(int wave)
    {
        foreach (var wd in _waveDatas)
            if (wd != null && wd.waveNumber == wave)
                return wd;
        return null;
    }
}