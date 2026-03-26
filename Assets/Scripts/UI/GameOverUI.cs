using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeDuration = 1.5f;

    [Header("Result Panel")]
    [SerializeField] private GameObject _resultPanel;

    [Header("Result Text")]
    [SerializeField] private TextMeshProUGUI _killText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _timeText;

    [Header("Button")]
    [SerializeField] private Button _backToTitleBtn;

    [Header("Scene")]
    [SerializeField] private string _titleSceneName = "Title";

    private PlayerStats _playerStats;

    private void Awake()
    {
        if (_fadeImage != null)
        {
            _fadeImage.color = new Color(0f, 0f, 0f, 0f);
            _fadeImage.gameObject.SetActive(false);
        }

        _resultPanel?.SetActive(false);
        _backToTitleBtn?.onClick.AddListener(OnBackToTitle);
    }

    private void Start()
    {
        _playerStats = FindFirstObjectByType<PlayerStats>();
        if (_playerStats != null)
            _playerStats.OnDie += Show;
    }

    private void OnDestroy()
    {
        if (_playerStats != null)
            _playerStats.OnDie -= Show;
    }

    public void Show()
    {
        if (_fadeImage != null)
        {
            _fadeImage.gameObject.SetActive(true);
            _fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }

        StartCoroutine(CoShow());
    }

    private IEnumerator CoShow()
    {
        BgmManager.Instance?.Stop();
        BgmManager.Instance?.PlayGameOver();

        GameStatsManager.Instance?.StopTracking();

        Time.timeScale = 0f;
        OptionsUI.IsOptionsOpen = true;

        if (_fadeImage != null)
        {
            _fadeImage.color = new Color(0f, 0f, 0f, 0f);
            _fadeImage.gameObject.SetActive(true);
        }

        float t = 0f;
        while (t < _fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 0.8f, t / _fadeDuration);
            if (_fadeImage != null)
                _fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        _resultPanel?.SetActive(true);
        RefreshStats();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RefreshStats()
    {
        if (GameStatsManager.Instance == null) return;

        if (_killText)
            _killText.text = $"총 처치 수 : <color=red>{GameStatsManager.Instance.TotalKills}</color>";

        if (_goldText)
            _goldText.text = $"총 수익 : <color=yellow>{GameStatsManager.Instance.TotalGoldEarned:N0} $</color>";

        if (_timeText)
        {
            float time = GameStatsManager.Instance.TotalSurvivalTime;
            int min = (int)(time / 60f);
            int sec = (int)(time % 60f);
            _timeText.text = $"생존 시간 : <color=#66CCFF>{min:D2}:{sec:D2}</color>";
        }
    }

    private void OnBackToTitle()
    {
        Time.timeScale = 1f;
        OptionsUI.IsOptionsOpen = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        BgmManager.Instance?.Stop();
        SceneManager.LoadScene(_titleSceneName);
    }
}