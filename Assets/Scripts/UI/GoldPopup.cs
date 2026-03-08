using TMPro;
using DG.Tweening;
using UnityEngine;

public class GoldPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private float _riseDuration = 1.2f;
    [SerializeField] private float _riseHeight = 80f;
    [SerializeField] private Ease _riseEase = Ease.OutCubic;

    private RectTransform _rect;
    private Camera _cam;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _cam = Camera.main;
    }

    public void Play(int amount, Vector3 worldPos)
    {
        _label.text = $"+{amount:N0}";
        _label.color = new Color(1f, 0.85f, 0.1f, 1f);

        // 월드 -> Canvas 좌표
        Canvas canvas = GetComponentInParent<Canvas>();
        _rect.anchoredPosition = WorldToCanvasPos(worldPos, canvas);

        // 위로 떠오르기 + 페이드아웃 동시 실행
        _rect.DOAnchorPosY(_rect.anchoredPosition.y + _riseHeight, _riseDuration)
             .SetEase(_riseEase);

        _label.DOFade(0f, _riseDuration)
              .SetEase(Ease.InQuad)
              .OnComplete(() => Destroy(gameObject));
    }

    private Vector2 WorldToCanvasPos(Vector3 world, Canvas canvas)
    {
        Vector2 screenPoint = _cam.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cam,
            out Vector2 local);
        return local;
    }
}