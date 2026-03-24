using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Images")]
    [SerializeField] private RectTransform _top;
    [SerializeField] private RectTransform _bottom;
    [SerializeField] private RectTransform _left;
    [SerializeField] private RectTransform _right;

    private Image _topImage;
    private Image _bottomImage;
    private Image _leftImage;
    private Image _rightImage;

    [Header("Refs")]
    [SerializeField] private GunController _gun;
    [SerializeField] private PlayerMoveCC _playerMove;

    [Header("Spread Settings")]
    [SerializeField] private float _baseOffset = 20f;
    [SerializeField] private float _maxSpreadOffset = 80f;
    [SerializeField] private float _lerpSpeed = 12f;

    [Header("WeaponData")]
    [SerializeField] private WeaponData _weaponData;

    private float _currentOffset;

    private void Awake()
    {
        if (_top) _topImage = _top.GetComponent<Image>();
        if (_bottom) _bottomImage = _bottom.GetComponent<Image>();
        if (_left) _leftImage = _left.GetComponent<Image>();
        if (_right) _rightImage = _right.GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (OptionsManager.Instance != null)
            OptionsManager.Instance.OnSettingsChanged += ApplyCrosshairColor;
        ApplyCrosshairColor();
    }

    private void OnDisable()
    {
        if (OptionsManager.Instance != null)
            OptionsManager.Instance.OnSettingsChanged -= ApplyCrosshairColor;
    }

    private void ApplyCrosshairColor()
    {
        if (OptionsManager.Instance == null) return;
        Color c = OptionsManager.Instance.CrosshairColor;
        if (_topImage) _topImage.color = c;
        if (_bottomImage) _bottomImage.color = c;
        if (_leftImage) _leftImage.color = c;
        if (_rightImage) _rightImage.color = c;
    }

    private void Update()
    {
        float spread = 0f;
        float maxSpread = 1f;
        if (_gun != null && _gun._data != null)
            maxSpread = Mathf.Max(_gun._data.maxSpread, 0.001f);
        spread = _gun != null ? _gun.CurrentSpread : 0f;
        float normalizedSpread = Mathf.Clamp01(spread / maxSpread);
        bool isAirborne = _playerMove != null && !_playerMove.IsGrounded;
        float targetOffset = isAirborne
            ? _baseOffset + _maxSpreadOffset
            : _baseOffset + normalizedSpread * _maxSpreadOffset;
        _currentOffset = Mathf.Lerp(_currentOffset, targetOffset, _lerpSpeed * Time.deltaTime);
        ApplyOffset(_currentOffset);
    }

    public void SetGunController(GunController gun)
    {
        _gun = gun;
    }

    public void SetSpread(float spread, float maxSpread)
    {
        float normalized = Mathf.Clamp01(spread / Mathf.Max(maxSpread, 0.001f));
        float target = _baseOffset + normalized * _maxSpreadOffset;
        _currentOffset = Mathf.Lerp(_currentOffset, target, _lerpSpeed * Time.deltaTime);
        ApplyOffset(_currentOffset);
    }

    private void ApplyOffset(float offset)
    {
        if (_top) _top.anchoredPosition = new Vector2(0f, offset);
        if (_bottom) _bottom.anchoredPosition = new Vector2(0f, -offset);
        if (_left) _left.anchoredPosition = new Vector2(-offset, 0f);
        if (_right) _right.anchoredPosition = new Vector2(offset, 0f);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyOffset(_baseOffset);
    }
#endif
}