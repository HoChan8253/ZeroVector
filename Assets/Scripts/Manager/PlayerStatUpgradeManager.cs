using System;
using UnityEngine;

// HP / Shield / Stamina 업그레이드 상태 관리
// 최대 5단계
public class PlayerStatUpgradeManager : MonoBehaviour
{
    public const int MAX_LEVEL = 5;

    [Header("연결")]
    [SerializeField] private PlayerStats _stats;

    [Header("HP 업그레이드")]
    [SerializeField] private int[] _hpPrices = new int[5];
    [SerializeField] private float _hpPerLevel = 20f;   // 단계당 최대 HP 증가량

    [Header("Shield 업그레이드")]
    [SerializeField] private int[] _sdPrices = new int[5];
    [SerializeField] private float _sdPerLevel = 10f;   // 단계당 최대 Shield 증가량

    [Header("Stamina Regen 업그레이드")]
    [SerializeField] private int[] _stPrices = new int[5];
    [SerializeField] private float _stRegenPerLevel = 5f;   // 단계당 스테미너 회복속도 증가량

    // 현재 단계
    public int HpLevel { get; private set; }
    public int SdLevel { get; private set; }
    public int StLevel { get; private set; }

    // 다음 가격
    public int NextHpPrice => GetNextPrice(_hpPrices, HpLevel);
    public int NextSdPrice => GetNextPrice(_sdPrices, SdLevel);
    public int NextStPrice => GetNextPrice(_stPrices, StLevel);

    public event Action OnStatsChanged;

    private void Awake()
    {
        if (_stats == null)
            _stats = FindFirstObjectByType<PlayerStats>();
    }

    // 업그레이드
    public bool TryUpgradeHp()
    {
        if (!TrySpend(_hpPrices, HpLevel)) return false;

        HpLevel++;
        _stats._maxHp += _hpPerLevel;
        // 최대치 증가분만큼만 현재 HP 보충
        _stats.Heal(_hpPerLevel);
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool TryUpgradeSd()
    {
        if (!TrySpend(_sdPrices, SdLevel)) return false;

        SdLevel++;
        _stats._maxShield += _sdPerLevel;
        _stats.HealShield(_sdPerLevel);
        OnStatsChanged?.Invoke();
        return true;
    }

    public bool TryUpgradeSt()
    {
        if (!TrySpend(_stPrices, StLevel)) return false;

        StLevel++;
        _stats._staminaRegenPerSec += _stRegenPerLevel;
        OnStatsChanged?.Invoke();
        return true;
    }

    private bool TrySpend(int[] prices, int level)
    {
        if (prices == null || level >= MAX_LEVEL) return false;
        if (GoldManager.Instance == null) return false;
        if (!GoldManager.Instance.CanAfford(prices[level])) return false;

        GoldManager.Instance.Spend(prices[level]);
        return true;
    }

    private static int GetNextPrice(int[] prices, int level)
    {
        if (prices == null || level >= MAX_LEVEL) return -1;
        return prices[level];
    }
}