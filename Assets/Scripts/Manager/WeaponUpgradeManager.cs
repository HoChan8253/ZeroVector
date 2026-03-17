using System;
using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    public const int MAX_LEVEL = 10;

    [SerializeField] private WeaponUpgradeData _upgradeData;

    public int PowerLevel;
    public int MagLevel;
    public int ThirdLevel;

    private bool _purchased;
    public bool IsOwned => _upgradeData != null && (_upgradeData.isPistol || _purchased);

    public WeaponUpgradeData Data => _upgradeData;
    public WeaponData WeaponData => _upgradeData != null ? _upgradeData.weaponData : null;

    // 최종 스탯
    public float CurrentDamage
    {
        get
        {
            if (WeaponData == null) return 0f;
            bool isShotgun = _upgradeData.isShotgun;
            float baseDmg = isShotgun ? WeaponData.damagePerPellet : WeaponData.damage;
            return baseDmg + _upgradeData.powerPerLevel * PowerLevel;
        }
    }

    // 탄창 크기 (AR/권총 전용)
    public int CurrentMagSize
    {
        get
        {
            if (WeaponData == null) return 0;
            if (_upgradeData.isShotgun) return WeaponData.magSize; // 샷건은 탄창 업그레이드 없음
            return WeaponData.magSize + _upgradeData.magPerLevel * MagLevel;
        }
    }

    // 펠릿 수 (샷건 전용)
    public int CurrentPelletCount
        => WeaponData != null
            ? WeaponData.pelletCount + _upgradeData.magPerLevel * MagLevel
            : 0;

    // 예비탄약 (AR/샷건)
    public int CurrentReserveAmmo
        => WeaponData != null
            ? WeaponData.startReserveAmmo + _upgradeData.thirdPerLevel * ThirdLevel
            : 0;

    // 치명타 확률 0~1 (권총 전용)
    public float CurrentCritChance
        => _upgradeData != null && _upgradeData.isPistol
            ? Mathf.Clamp01(_upgradeData.baseCritChance + _upgradeData.thirdPerLevel * ThirdLevel * 0.01f)
            : 0f;

    public event Action OnStatsChanged;
    public event Action OnWeaponPurchased;

    // 무기 구매
    public bool TryPurchaseWeapon()
    {
        if (IsOwned || _upgradeData == null) return false;
        if (GoldManager.Instance == null) return false;
        if (!GoldManager.Instance.CanAfford(_upgradeData.purchasePrice)) return false;

        GoldManager.Instance.Spend(_upgradeData.purchasePrice);
        _purchased = true;
        OnWeaponPurchased?.Invoke();
        OnStatsChanged?.Invoke();
        return true;
    }

    // 업그레이드
    public bool TryUpgradePower() => TryUpgrade(ref PowerLevel, _upgradeData?.powerPrices);
    public bool TryUpgradeMag() => TryUpgrade(ref MagLevel, _upgradeData?.magPrices);
    public bool TryUpgradeThird() => TryUpgrade(ref ThirdLevel, _upgradeData?.thirdPrices);

    public int NextPowerPrice => GetNextPrice(_upgradeData?.powerPrices, PowerLevel);
    public int NextMagPrice => GetNextPrice(_upgradeData?.magPrices, MagLevel);
    public int NextThirdPrice => GetNextPrice(_upgradeData?.thirdPrices, ThirdLevel);

    private bool TryUpgrade(ref int level, int[] prices)
    {
        if (!IsOwned) return false;
        if (prices == null || level >= MAX_LEVEL) return false;
        if (GoldManager.Instance == null) return false;

        int price = prices[level];
        if (!GoldManager.Instance.CanAfford(price)) return false;

        GoldManager.Instance.Spend(price);
        level++;
        OnStatsChanged?.Invoke();
        return true;
    }

    private static int GetNextPrice(int[] prices, int currentLevel)
    {
        if (prices == null || currentLevel >= MAX_LEVEL) return -1;
        return prices[currentLevel];
    }
}