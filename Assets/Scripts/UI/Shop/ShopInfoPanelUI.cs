using UnityEngine;
using DG.Tweening;

public class ShopInfoPanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform _panel;

    [Header("애니메이션")]
    [SerializeField] private float _duration = 0.4f;
    [SerializeField] private Ease _easeIn = Ease.OutBack;
    [SerializeField] private Ease _easeOut = Ease.InBack;

    [Header("슬라이드 방향")]
    [Tooltip("화면 밖으로 숨길 방향 오프셋")]
    [SerializeField] private Vector2 _hiddenOffset = new Vector2(0f, -120f);

    private Vector2 _shownPos;
    private Tweener _tween;

    private void Awake()
    {
        if (_panel == null)
            _panel = GetComponent<RectTransform>();

        _shownPos = _panel.anchoredPosition;
    }

    private void Start()
    {
        var dnm = DayNightManager.Instance;
        if (dnm != null)
        {
            dnm.OnDayStart += HandleDayStart;
            dnm.OnNightStart += HandleNightStart;

            // 현재 상태에 맞게 초기 위치 설정
            if (dnm.IsNight)
                _panel.anchoredPosition = _shownPos + _hiddenOffset;
            else
                _panel.anchoredPosition = _shownPos;
        }
    }

    private void OnDestroy()
    {
        var dnm = DayNightManager.Instance;
        if (dnm != null)
        {
            dnm.OnDayStart -= HandleDayStart;
            dnm.OnNightStart -= HandleNightStart;
        }

        _tween?.Kill();
    }

    private void HandleDayStart()
    {
        _tween?.Kill();
        _tween = _panel.DOAnchorPos(_shownPos, _duration)
                       .SetEase(_easeIn)
                       .SetUpdate(true); // timeScale 0 에서도 동작
    }

    private void HandleNightStart()
    {
        _tween?.Kill();
        _tween = _panel.DOAnchorPos(_shownPos + _hiddenOffset, _duration)
                       .SetEase(_easeOut)
                       .SetUpdate(true);
    }
}