using TMPro;
using UnityEngine;
using System.Collections;

public class SafeZoneManager : MonoBehaviour
{
    [Header("Zone Config")]
    [SerializeField] private Transform[] _zone; // 후보 지점들
    [SerializeField] private float _startRadius = 100f;
    [SerializeField] private float _endRadius = 32.5f;
    [SerializeField] private float _shrinkDelay = 30f; // 축소 시작까지 대기
    [SerializeField] private float _shrinkDuration = 40f; // 축소에 걸리는 시간
    [SerializeField] private float _damagePerSecond = 5f;
    [SerializeField] private float _damageInterval = 1f;

    [Header("Visual")]
    [SerializeField] private LineRenderer _circleRenderer; // 원 테두리 표시
    [SerializeField] private int _circleSegments = 64; // 부드러운 원

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private CanvasGroup _statusGroup;
    [SerializeField] private float _textDisplayTime = 3f;
    [SerializeField] private float _warningInterval = 5f;

    [Header("Refs")]
    [SerializeField] private Transform _player;
    [SerializeField] private IDamageable _playerDamageable;

    [Header("Beacon")]
    [SerializeField] private ZoneBeacon _beacon;

    private Vector3 _nextCenter;
    private float _currentRadius;
    private bool _isActive;
    private bool _isShrinking;

    private Coroutine _damageCoroutine;
    private Coroutine _textCoroutine;

    private void Start()
    {
        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                _player = go.transform;
                _playerDamageable = go.GetComponentInChildren<IDamageable>();
            }
        }

        DayNightManager.Instance.OnNightStart += OnNightStart;
        DayNightManager.Instance.OnDayStart += OnDayEnd;
    }

    private void OnDestroy()
    {
        if (DayNightManager.Instance == null) return;
        DayNightManager.Instance.OnNightStart -= OnNightStart;
        DayNightManager.Instance.OnDayStart -= OnDayEnd;
    }

    private void OnNightStart()
    {
        _currentRadius = _startRadius;
        
        // 후보 중 랜덤 선택
        _nextCenter = _zone.Length > 0
            ? _zone[Random.Range(0, _zone.Length)].position
            : transform.position;
        _nextCenter.y = 0f;

        _isActive = true;
        DrawCircle(_nextCenter, _currentRadius);

        // 비콘 표시
        _beacon?.Show(_nextCenter);

        StopAllCoroutines();
        StartCoroutine(CoShrink());
        _damageCoroutine = StartCoroutine(CoDamageLoop());
    }

    private void OnDayEnd()
    {
        _isActive = false;
        _isShrinking = false;
        StopAllCoroutines();
        _circleRenderer.enabled = false;
        if (_statusGroup != null) _statusGroup.alpha = 0f;
    }

    private IEnumerator CoShrink()
    {
        float elapsed = 0f;
        while (elapsed < _shrinkDelay)
        {
            ShowText("안전 구역 축소 중", _textDisplayTime);
            yield return new WaitForSeconds(_warningInterval);
            elapsed += _warningInterval;
        }

        // 축소 시작 전 비콘 숨김
        _beacon?.Hide();

        ShowText("다음 구역으로 이동하세요", _textDisplayTime);

        _isShrinking = true;
        float shrinkElapsed = 0f;
        while (shrinkElapsed < _shrinkDuration)
        {
            shrinkElapsed += Time.deltaTime;
            float t = shrinkElapsed / _shrinkDuration;
            _currentRadius = Mathf.Lerp(_startRadius, _endRadius, t);
            DrawCircle(_nextCenter, _currentRadius);
            yield return null;
        }

        _currentRadius = _endRadius;
        _isShrinking = false;
    }

    private IEnumerator CoDamageLoop()
    {
        while (_isActive)
        {
            yield return new WaitForSeconds(_damageInterval);
            if (_player == null) continue;

            float dist = Vector3.Distance(
                new Vector3(_player.position.x, 0, _player.position.z),
                new Vector3(_nextCenter.x, 0, _nextCenter.z));

            if (dist > _currentRadius)
            {
                ShowText("구역 밖입니다!", 2f);
                _playerDamageable?.TakeDamage((int)(_damagePerSecond * _damageInterval));
            }
        }
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        if (_circleRenderer == null) return;
        _circleRenderer.enabled = true;
        _circleRenderer.positionCount = _circleSegments + 1;
        _circleRenderer.loop = true;

        for (int i = 0; i <= _circleSegments; i++)
        {
            float angle = i * 2f * Mathf.PI / _circleSegments;
            float x = center.x + Mathf.Cos(angle) * radius;
            float z = center.z + Mathf.Sin(angle) * radius;
            _circleRenderer.SetPosition(i, new Vector3(x, 0.1f, z));
        }
    }

    private void ShowText(string msg, float duration)
    {
        if (_textCoroutine != null) StopCoroutine(_textCoroutine);
        _textCoroutine = StartCoroutine(CoShowText(msg, duration));
    }

    private IEnumerator CoShowText(string msg, float duration)
    {
        _statusText.text = msg;
        _statusGroup.alpha = 1f;
        yield return new WaitForSeconds(duration);
        _statusGroup.alpha = 0f;
    }
}