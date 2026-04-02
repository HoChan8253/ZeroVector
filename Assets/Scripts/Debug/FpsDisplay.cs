using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _fpsText;

    [Header("옵션")]
    [Tooltip("FPS 갱신 주기 (초)")]
    [SerializeField] private float _updateInterval = 0.5f;

    private float _timer;
    private int _fps;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer < _updateInterval) return;

        _fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        _timer = 0f;

        if (_fpsText == null) return;

        // FPS 에 따라 색상 변경
        _fpsText.color = _fps switch
        {
            >= 60 => new Color(0.4f, 1f, 0.4f),
            >= 30 => new Color(1f, 0.85f, 0.2f),
            _ => new Color(1f, 0.3f, 0.3f),
        };

        _fpsText.text = $"FPS  {_fps}";
    }
}