using TMPro;
using UnityEngine;
using UnityEngine.UI;

// PlayerStatPanel UI 전체 제어
// HP / Shield / Stamina 업그레이드 + 현재 스탯 표시
public class PlayerStatPanelUI : MonoBehaviour
{
    [Header("매니저")]
    [SerializeField] private PlayerStatUpgradeManager _upgradeManager;
    [SerializeField] private PlayerStats _stats;

    [Header("HP 슬롯")]
    [SerializeField] private Image[] _hpDots;
    [SerializeField] private TextMeshProUGUI _hpPriceText;
    [SerializeField] private Button _hpBuyBtn;

    [Header("Shield 슬롯")]
    [SerializeField] private Image[] _sdDots;
    [SerializeField] private TextMeshProUGUI _sdPriceText;
    [SerializeField] private Button _sdBuyBtn;

    [Header("Stamina Regen 슬롯")]
    [SerializeField] private Image[] _stDots;
    [SerializeField] private TextMeshProUGUI _stPriceText;
    [SerializeField] private Button _stBuyBtn;

    [Header("HP StatsPanel")]
    [SerializeField] private TextMeshProUGUI _minHpText;
    [SerializeField] private TextMeshProUGUI _maxHpText;

    [Header("Shield StatsPanel")]
    [SerializeField] private TextMeshProUGUI _minSdText;
    [SerializeField] private TextMeshProUGUI _maxSdText;

    [Header("Stamina StatsPanel")]
    [SerializeField] private TextMeshProUGUI _stRegenText;

    [Header("색상")]
    [SerializeField] private Color _dotOff = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color _dotOn = new Color(0.1f, 0.9f, 0.2f);

    private void Awake()
    {
        if (_stats == null)
            _stats = FindFirstObjectByType<PlayerStats>();

        _hpBuyBtn?.onClick.AddListener(OnHpBuyClicked);
        _sdBuyBtn?.onClick.AddListener(OnSdBuyClicked);
        _stBuyBtn?.onClick.AddListener(OnStBuyClicked);

        if (_upgradeManager != null)
            _upgradeManager.OnStatsChanged += Refresh;

        if (_stats != null)
        {
            _stats.OnHpChanged += (cur, max) => RefreshStatTexts();
            _stats.OnShieldChanged += (cur, max) => RefreshStatTexts();
        }
    }

    private void OnDestroy()
    {
        if (_upgradeManager != null)
            _upgradeManager.OnStatsChanged -= Refresh;
    }

    private void Start() => Refresh();

    private void OnHpBuyClicked()
    {
        if (_upgradeManager != null && !_upgradeManager.TryUpgradeHp())
            Debug.Log("[StatUI] HP 업그레이드 실패 (골드 부족 or 만렙)");
    }

    private void OnSdBuyClicked()
    {
        if (_upgradeManager != null && !_upgradeManager.TryUpgradeSd())
            Debug.Log("[StatUI] Shield 업그레이드 실패");
    }

    private void OnStBuyClicked()
    {
        if (_upgradeManager != null && !_upgradeManager.TryUpgradeSt())
            Debug.Log("[StatUI] Stamina 업그레이드 실패");
    }

    private void Refresh()
    {
        if (_upgradeManager == null) return;

        RefreshSlot(_hpDots, _hpPriceText, _hpBuyBtn, _upgradeManager.HpLevel, _upgradeManager.NextHpPrice);
        RefreshSlot(_sdDots, _sdPriceText, _sdBuyBtn, _upgradeManager.SdLevel, _upgradeManager.NextSdPrice);
        RefreshSlot(_stDots, _stPriceText, _stBuyBtn, _upgradeManager.StLevel, _upgradeManager.NextStPrice);
        RefreshStatTexts();
    }

    private void RefreshStatTexts()
    {
        if (_stats == null) return;

        if (_minHpText != null) _minHpText.text = $"{_stats.Hp:F0}";
        if (_maxHpText != null) _maxHpText.text = $"{_stats._maxHp:F0}";
        if (_minSdText != null) _minSdText.text = $"{_stats.Shield:F0}";
        if (_maxSdText != null) _maxSdText.text = $"{_stats._maxShield:F0}";
        if (_stRegenText != null) _stRegenText.text = $"{_stats._staminaRegenPerSec:F1}";
    }

    private void RefreshSlot(Image[] dots, TextMeshProUGUI priceText, Button buyBtn, int level, int nextPrice)
    {
        if (dots != null)
            for (int i = 0; i < dots.Length; i++)
                if (dots[i] != null)
                    dots[i].color = i < level ? _dotOn : _dotOff;

        bool maxed = level >= PlayerStatUpgradeManager.MAX_LEVEL;

        if (priceText != null)
            priceText.text = maxed ? "MAX" : $"Price : {nextPrice:N0}";

        buyBtn?.gameObject.SetActive(!maxed);
    }
}