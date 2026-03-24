using TMPro;
using DG.Tweening;
using UnityEngine;

public class GoldPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private float _riseDuration = 1.2f;
    [SerializeField] private float _riseHeight = 80f;
    [SerializeField] private Ease _riseEase = Ease.OutCubic;
    [SerializeField] private Vector2 _anchoredPosition = new Vector2(-20f, -130f);

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void Play(int amount, Vector3 worldPos, Camera cam)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Destroy(gameObject);
            return;
        }

        _label.text = $"+{amount:N0}";
        _label.color = new Color(1f, 0.85f, 0.1f, 1f);

        _rect.anchorMin = new Vector2(1f, 1f);
        _rect.anchorMax = new Vector2(1f, 1f);
        _rect.pivot = new Vector2(1f, 1f);
        _rect.anchoredPosition = _anchoredPosition;

        _rect.DOAnchorPosY(_anchoredPosition.y + _riseHeight, _riseDuration)
             .SetEase(_riseEase);

        _label.DOFade(0f, _riseDuration)
              .SetEase(Ease.InQuad)
              .OnComplete(() => Destroy(gameObject));
    }
}