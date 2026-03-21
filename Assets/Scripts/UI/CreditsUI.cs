using UnityEngine;
using UnityEngine.UI;

public class CreditsUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _creditsPanel;

    [Header("버튼")]
    [SerializeField] private Button _closeBtn;

    [Header("타이틀 메뉴")]
    [SerializeField] private TitleMenuUI _titleMenuUI;

    public bool IsOpen => _creditsPanel != null && _creditsPanel.activeSelf;

    private void Awake()
    {
        _closeBtn?.onClick.AddListener(Close);
    }

    public void Open()
    {
        _creditsPanel?.SetActive(true);
    }

    public void Close()
    {
        _creditsPanel?.SetActive(false);
        _titleMenuUI?.OnPanelClosed();
    }
}