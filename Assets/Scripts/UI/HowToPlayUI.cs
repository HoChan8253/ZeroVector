using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _howToPlayPanel;
    [SerializeField] private GameObject[] _pages;

    [Header("버튼")]
    [SerializeField] private Button _closeBtn;
    [SerializeField] private Button _prevBtn;
    [SerializeField] private Button _nextBtn;

    [Header("페이지 텍스트")]
    [SerializeField] private TextMeshProUGUI _pageText;

    [Header("타이틀 메뉴")]
    [SerializeField] private TitleMenuUI _titleMenuUI;

    private int _currentPage = 0;
    public bool IsOpen => _howToPlayPanel != null && _howToPlayPanel.activeSelf;

    private void Awake()
    {
        _closeBtn?.onClick.AddListener(Close);
        _prevBtn?.onClick.AddListener(OnPrev);
        _nextBtn?.onClick.AddListener(OnNext);
    }

    public void Open()
    {
        _currentPage = 0;
        _howToPlayPanel?.SetActive(true);
        RefreshPage();
    }

    public void Close()
    {
        _howToPlayPanel?.SetActive(false);
        _titleMenuUI?.OnPanelClosed();
    }

    private void OnPrev()
    {
        if (_currentPage <= 0) return;
        _currentPage--;
        RefreshPage();
    }

    private void OnNext()
    {
        if (_currentPage >= _pages.Length - 1) return;
        _currentPage++;
        RefreshPage();
    }

    private void RefreshPage()
    {
        // 현재 페이지만 활성화
        for (int i = 0; i < _pages.Length; i++)
            _pages[i]?.SetActive(i == _currentPage);

        // 페이지 텍스트
        if (_pageText)
            _pageText.text = $"{_currentPage + 1} / {_pages.Length}";

        // Prev/Next 버튼 인터랙션
        if (_prevBtn) _prevBtn.interactable = _currentPage > 0;
        if (_nextBtn) _nextBtn.interactable = _currentPage < _pages.Length - 1;
    }
}