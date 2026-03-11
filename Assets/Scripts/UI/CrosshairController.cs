using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Images")]
    [SerializeField] private RectTransform _top;
    [SerializeField] private RectTransform _bottom;
    [SerializeField] private RectTransform _left;
    [SerializeField] private RectTransform _right;

    [Header("Refs")]
    [SerializeField] private GunController _gun;

    [Header("Spread Settings")]
    [Tooltip("spread 0일 때 크로스헤어 중심에서의 기본 거리")]
    [SerializeField] private float _baseOffset = 20f;

    [Tooltip("maxSpread일 때 추가로 벌어지는 최대 거리")]
    [SerializeField] private float _maxSpreadOffset = 80f;

    [Tooltip("크로스헤어가 벌어지고 좁혀지는 보간 속도")]
    [SerializeField] private float _lerpSpeed = 12f;

    [Header("WeaponData")]
    [SerializeField] private WeaponData _weaponData;

    private float _currentOffset;

    private void Update()
    {
        float spread = 0f;
        float maxSpread = 1f;

        if (_gun != null && _gun._data != null)
        {
            maxSpread = Mathf.Max(_gun._data.maxSpread, 0.001f);
        }

        spread = _gun != null ? _gun.CurrentSpread : 0f;

        float normalizedSpread = Mathf.Clamp01(spread / maxSpread);
        float targetOffset = _baseOffset + normalizedSpread * _maxSpreadOffset;

        _currentOffset = Mathf.Lerp(_currentOffset, targetOffset, _lerpSpeed * Time.deltaTime);

        ApplyOffset(_currentOffset);
    }

    public void SetGunController(GunController gun)
    {
        _gun = gun;
    }

    // GunController에서 직접 spread 값을 넘겨받을 때 호출
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