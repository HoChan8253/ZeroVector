using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Tab : 상점 열기
// Escape 또는 X 버튼 : 상점 닫기
public class ShopPanelUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private Button _closeButton;
    [SerializeField] private PlayerInputHub _input;

    [Header("밤 경고 문구")]
    [SerializeField] private TextMeshProUGUI _nightWarningText;
    [SerializeField] private float _warningFadeInTime = 0.2f;
    [SerializeField] private float _warningHoldTime = 1.2f;
    [SerializeField] private float _warningFadeOutTime = 0.4f;

    [Header("옵션")]
    [SerializeField] private bool _onlyDaytime = true;

    public static bool IsOpen { get; private set; }

    private Coroutine _warningCo;

    private void Awake()
    {
        if (_input == null)
            _input = FindFirstObjectByType<PlayerInputHub>();

        _closeButton?.onClick.AddListener(CloseShop);
        _shopPanel?.SetActive(false);

        // 경고 텍스트 초기 숨김
        if (_nightWarningText != null)
        {
            var c = _nightWarningText.color;
            c.a = 0f;
            _nightWarningText.color = c;
        }
    }

    private void Update()
    {
        if (_input == null) return;

        if (_input.ShopPressedThisFrame)
        {
            if (_shopPanel != null && _shopPanel.activeSelf)
                CloseShop();
            else
                TryOpenShop();
        }

        if (_input.CancelPressedThisFrame && _shopPanel != null && _shopPanel.activeSelf)
        {
            _input.CancelConsumed = true;
            CloseShop();
        }
    }

    private void TryOpenShop()
    {
        if (_onlyDaytime)
        {
            bool isNight = DayNightManager.Instance != null && DayNightManager.Instance.IsNight;
            if (isNight)
            {
                ShowNightWarning();
                return;
            }
        }

        OpenShop();
    }

    private void OpenShop()
    {
        IsOpen = true;
        _shopPanel?.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseShop()
    {
        IsOpen = false;
        _shopPanel?.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 밤 경고 문구
    private void ShowNightWarning()
    {
        if (_nightWarningText == null) return;

        if (_warningCo != null)
            StopCoroutine(_warningCo);

        _warningCo = StartCoroutine(CoWarningFlash());
    }

    private IEnumerator CoWarningFlash()
    {
        _nightWarningText.text = "상점은 낮에만 이용할 수 있습니다.";

        // Fade In
        yield return CoFadeText(_nightWarningText, 0f, 1f, _warningFadeInTime);

        // Hold
        yield return new WaitForSecondsRealtime(_warningHoldTime);

        // Fade Out
        yield return CoFadeText(_nightWarningText, 1f, 0f, _warningFadeOutTime);

        _warningCo = null;
    }

    private IEnumerator CoFadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            var c = text.color;
            c.a = alpha;
            text.color = c;
            yield return null;
        }

        var final = text.color;
        final.a = to;
        text.color = final;
    }
}