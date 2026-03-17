using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUpgradeUI : MonoBehaviour
{
    [Header("매니저")]
    [SerializeField] private WeaponUpgradeManager _upgradeManager;

    [Header("구매 전 (AR/샷건 전용)")]
    [SerializeField] private Button _registerButton;

    [Header("구매 후 패널")]
    [SerializeField] private GameObject _buyPanel;
    [SerializeField] private GameObject _statsPanel;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI _statPowerText;
    [SerializeField] private TextMeshProUGUI _statMagText;
    [SerializeField] private TextMeshProUGUI _statThirdText;

    [Header("Power 슬롯")]
    [SerializeField] private Image[] _powerDots;
    [SerializeField] private TextMeshProUGUI _powerPriceText;
    [SerializeField] private Button _powerBuyBtn;

    [Header("Mag / Pellet 슬롯")]
    [SerializeField] private Image[] _magDots;
    [SerializeField] private TextMeshProUGUI _magPriceText;
    [SerializeField] private Button _magBuyBtn;

    [Header("3번째 슬롯 (Ammo / Crit)")]
    [SerializeField] private Image[] _thirdDots;
    [SerializeField] private TextMeshProUGUI _thirdPriceText;
    [SerializeField] private Button _thirdBuyBtn;

    [Header("색상")]
    [SerializeField] private Color _dotOff = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color _dotOn = new Color(0.1f, 0.9f, 0.2f);

    private void Awake()
    {
        _registerButton?.onClick.AddListener(OnRegisterClicked);
        _powerBuyBtn?.onClick.AddListener(OnPowerBuyClicked);
        _magBuyBtn?.onClick.AddListener(OnMagBuyClicked);
        _thirdBuyBtn?.onClick.AddListener(OnThirdBuyClicked);

        if (_upgradeManager != null)
        {
            _upgradeManager.OnStatsChanged += Refresh;
            _upgradeManager.OnWeaponPurchased += HandleWeaponPurchased;
        }
    }

    private void OnDestroy()
    {
        if (_upgradeManager != null)
        {
            _upgradeManager.OnStatsChanged -= Refresh;
            _upgradeManager.OnWeaponPurchased -= HandleWeaponPurchased;
        }
    }

    private void Start()
    {
        InitLayout();
        Refresh();
    }

    private void InitLayout()
    {
        if (_upgradeManager == null) return;

        bool isPistol = _upgradeManager.Data != null && _upgradeManager.Data.isPistol;

        if (isPistol)
        {
            _buyPanel?.SetActive(true);
            _statsPanel?.SetActive(true);
        }
        else
        {
            _buyPanel?.SetActive(false);
            _statsPanel?.SetActive(false);
        }
    }

    private void OnRegisterClicked()
    {
        _upgradeManager?.TryPurchaseWeapon();
    }

    private void HandleWeaponPurchased()
    {
        _registerButton?.gameObject.SetActive(false);
        _buyPanel?.SetActive(true);
        _statsPanel?.SetActive(true);
    }

    private void OnPowerBuyClicked()
    {
        _upgradeManager?.TryUpgradePower();
    }

    private void OnMagBuyClicked()
    {
        _upgradeManager?.TryUpgradeMag();
    }

    private void OnThirdBuyClicked()
    {
        _upgradeManager?.TryUpgradeThird();
    }

    private void Refresh()
    {
        if (_upgradeManager == null) return;

        RefreshStats();
        RefreshSlot(_powerDots, _powerPriceText, _powerBuyBtn, _upgradeManager.PowerLevel, _upgradeManager.NextPowerPrice);
        RefreshSlot(_magDots, _magPriceText, _magBuyBtn, _upgradeManager.MagLevel, _upgradeManager.NextMagPrice);
        RefreshSlot(_thirdDots, _thirdPriceText, _thirdBuyBtn, _upgradeManager.ThirdLevel, _upgradeManager.NextThirdPrice);
    }

    private void RefreshStats()
    {
        if (_upgradeManager == null) return;

        bool isPistol = _upgradeManager.Data != null && _upgradeManager.Data.isPistol;
        bool isShotgun = _upgradeManager.Data != null && _upgradeManager.Data.isShotgun;

        if (_statPowerText != null)
            _statPowerText.text = $"Pow : {_upgradeManager.CurrentDamage:F0}";

        if (_statMagText != null)
            _statMagText.text = isShotgun
                ? $"Pellet : {_upgradeManager.CurrentPelletCount}"
                : $"Mag : {_upgradeManager.CurrentMagSize}";

        if (_statThirdText != null)
            _statThirdText.text = isPistol
                ? $"Crit : {_upgradeManager.CurrentCritChance * 100f:F0}%"
                : $"Ammo : {_upgradeManager.CurrentReserveAmmo}";
    }

    private void RefreshSlot(Image[] dots, TextMeshProUGUI priceText, Button buyBtn, int level, int nextPrice)
    {
        if (dots != null)
            for (int i = 0; i < dots.Length; i++)
                if (dots[i] != null)
                    dots[i].color = i < level ? _dotOn : _dotOff;

        bool maxed = level >= WeaponUpgradeManager.MAX_LEVEL;

        if (priceText != null)
            priceText.text = maxed ? "MAX" : $"Price : {nextPrice:N0}";

        buyBtn?.gameObject.SetActive(!maxed);
    }
}