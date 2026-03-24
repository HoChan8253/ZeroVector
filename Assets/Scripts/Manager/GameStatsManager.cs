using UnityEngine;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    public int TotalKills { get; private set; }
    public int TotalGoldEarned { get; private set; }
    public float TotalSurvivalTime { get; private set; }

    private bool _isTracking;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _isTracking = true;

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded += OnGoldAdded;
    }

    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded -= OnGoldAdded;
    }

    private void Update()
    {
        if (!_isTracking) return;
        TotalSurvivalTime += Time.deltaTime;
    }

    private void OnGoldAdded(int total, int gained, Vector3 pos)
    {
        if (gained > 0)
            TotalGoldEarned += gained;
    }

    public void AddKill() => TotalKills++;

    public void StopTracking() => _isTracking = false;
}