using UnityEngine;
using DG.Tweening;

public class StatsBarAnim : MonoBehaviour
{
    [Header("Rects")]
    [SerializeField] private RectTransform _front;
    [SerializeField] private RectTransform _back;

    [Header("FillMask")]
    [SerializeField] private RectTransform _mask;

    [Header("Tween")]
    [SerializeField] private float _backDelay = 0.12f;
    [SerializeField] private float _backDuration = 0.45f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    private Tween _backTween;

    public void Set01(float ratio01)
    {
        ratio01 = Mathf.Clamp01(ratio01);
        if (_front == null || _back == null || _mask == null) return;

        float w = _mask.rect.width;                 // 마스크 폭
        float targetX = -w * (1f - ratio01);         // 1 -> 0, 0 -> -w

        float prevX = _front.anchoredPosition.x;

        // Front는 즉시 반영
        _front.anchoredPosition = new Vector2(targetX, _front.anchoredPosition.y);

        // 회복(체력 증가)이면 Back도 즉시 맞추기
        if (targetX >= prevX)
        {
            KillTween();
            _back.anchoredPosition = new Vector2(targetX, _back.anchoredPosition.y);
            return;
        }

        // 피격(체력 감소)이면 Back이 딜레이 후 따라오기
        KillTween();
        _backTween = _back
            .DOAnchorPosX(targetX, _backDuration)
            .SetDelay(_backDelay)
            .SetEase(_ease)
            .SetUpdate(true);
    }

    private void KillTween()
    {
        if (_backTween != null && _backTween.IsActive())
            _backTween.Kill();
        _backTween = null;
    }

    private void OnDisable()
    {
        KillTween();
    }
}