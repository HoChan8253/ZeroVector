using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopFillAmmoUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _fillAmmoBtn;

    [Header("무기 업그레이드 매니저")]
    [SerializeField] private WeaponUpgradeManager _arUpgradeManager;
    [SerializeField] private WeaponUpgradeManager _shotgunUpgradeManager;

    [Header("GunController")]
    [SerializeField] private GunController _arGunController;
    [SerializeField] private GunController _shotgunGunController;

    [Header("탄약 가격")]
    [SerializeField] private int _arAmmoPrice = 5;
    [SerializeField] private int _shotgunAmmoPrice = 15;

    [Header("가격 표시")]
    [SerializeField] private TextMeshProUGUI _priceText;

    private void Awake()
    {
        _fillAmmoBtn?.onClick.AddListener(OnFillAmmo);
    }

    private void Update()
    {
        RefreshPriceText();
    }

    private void OnFillAmmo()
    {
        if (GoldManager.Instance == null) return;

        int totalCost = CalculateTotalCost();
        if (totalCost <= 0) return;

        if (!GoldManager.Instance.CanAfford(totalCost))
        {
            FillPartial();
            return;
        }

        GoldManager.Instance.Spend(totalCost);
        FillFull();
    }

    // 전체 충전
    private void FillFull()
    {
        if (_arUpgradeManager != null && _arUpgradeManager.IsOwned)
            _arGunController?.FillReserveAmmo(GetArNeed());

        if (_shotgunUpgradeManager != null && _shotgunUpgradeManager.IsOwned)
            _shotgunGunController?.FillReserveAmmo(GetShotgunNeed());
    }

    // 골드가 부족할 때 살 수 있는 만큼만
    private void FillPartial()
    {
        int gold = GoldManager.Instance.Gold;

        // AR 먼저
        if (_arUpgradeManager != null && _arUpgradeManager.IsOwned)
        {
            int arNeed = GetArNeed();
            int arAffordable = Mathf.Min(arNeed, gold / _arAmmoPrice);
            if (arAffordable > 0)
            {
                int cost = arAffordable * _arAmmoPrice;
                GoldManager.Instance.Spend(cost);
                _arGunController?.FillReserveAmmo(arAffordable);
                gold -= cost;
            }
        }

        // 샷건
        if (_shotgunUpgradeManager != null && _shotgunUpgradeManager.IsOwned)
        {
            int shotgunNeed = GetShotgunNeed();
            int shotgunAffordable = Mathf.Min(shotgunNeed, gold / _shotgunAmmoPrice);
            if (shotgunAffordable > 0)
            {
                int cost = shotgunAffordable * _shotgunAmmoPrice;
                GoldManager.Instance.Spend(cost);
                _shotgunGunController?.FillReserveAmmo(shotgunAffordable);
            }
        }
    }

    private int CalculateTotalCost()
    {
        int total = 0;

        if (_arUpgradeManager != null && _arUpgradeManager.IsOwned)
            total += GetArNeed() * _arAmmoPrice;

        if (_shotgunUpgradeManager != null && _shotgunUpgradeManager.IsOwned)
            total += GetShotgunNeed() * _shotgunAmmoPrice;

        return total;
    }

    private int GetArNeed()
    {
        if (_arGunController == null || _arUpgradeManager == null) return 0;
        if (!_arUpgradeManager.IsOwned) return 0;

        int max = _arUpgradeManager.CurrentReserveAmmo;

        int current = _arGunController.gameObject.activeSelf
            ? _arGunController.ReserveAmmo
            : max;

        return Mathf.Max(0, max - current);
    }

    private int GetShotgunNeed()
    {
        if (_shotgunGunController == null || _shotgunUpgradeManager == null) return 0;
        if (!_shotgunUpgradeManager.IsOwned) return 0;

        int max = _shotgunUpgradeManager.CurrentReserveAmmo;

        int current = _shotgunGunController.gameObject.activeSelf
            ? _shotgunGunController.ReserveAmmo
            : max;

        return Mathf.Max(0, max - current);
    }

    private void RefreshPriceText()
    {
        if (_priceText == null) return;
        int cost = CalculateTotalCost();
        _priceText.text = cost > 0 ? $"{cost:N0} G" : "Full";
    }
}