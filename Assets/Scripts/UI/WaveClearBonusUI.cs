using TMPro;
using UnityEngine;
using System.Collections;

public class WaveClearBonusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private float _holdDuration = 2.0f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private int _blinkCount = 3;

    private Coroutine _showCoroutine;

    private void Awake()
    {
        Color c = _label.color;
        c.a = 0f;
        _label.color = c;
    }

    public void Show(int waveNumber, int bonusAmount)
    {
        _label.text = $"Wave {waveNumber} Clear!\n+{bonusAmount:N0} G";

        gameObject.SetActive(true);

        if (_showCoroutine != null)
            StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(CoShow());
    }

    private IEnumerator CoShow()
    {
        gameObject.SetActive(true);

        // 페이드 인
        yield return CoFade(0f, 1f, _fadeDuration);

        // 반짝임
        for (int i = 0; i < _blinkCount; i++)
        {
            yield return CoFade(1f, 0.2f, 0.12f);
            yield return CoFade(0.2f, 1f, 0.12f);
        }

        // 유지
        yield return new WaitForSeconds(_holdDuration);

        // 페이드 아웃
        yield return CoFade(1f, 0f, _fadeDuration);

        gameObject.SetActive(false);
        _showCoroutine = null;
    }

    private IEnumerator CoFade(float from, float to, float duration)
    {
        float t = 0f;
        Color c = _label.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            _label.color = c;
            yield return null;
        }
        c.a = to;
        _label.color = c;
    }
}