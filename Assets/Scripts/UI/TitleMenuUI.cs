using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TitleMenuUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _gameStartBtn;
    [SerializeField] private Button _howToPlayBtn;
    [SerializeField] private Button _optionsBtn;
    [SerializeField] private Button _creditsBtn;
    [SerializeField] private Button _quitBtn;

    [Header("패널")]
    [SerializeField] private HowToPlayUI _howToPlayUI;
    [SerializeField] private OptionsUI _optionsUI;
    [SerializeField] private CreditsUI _creditsUI;

    [Header("씬 이름")]
    [SerializeField] private string _gameSceneName = "InGame";

    private void Awake()
    {
        _gameStartBtn?.onClick.AddListener(OnGameStart);
        _howToPlayBtn?.onClick.AddListener(OnHowToPlay);
        _optionsBtn?.onClick.AddListener(OnOptions);
        _creditsBtn?.onClick.AddListener(OnCredits);
        _quitBtn?.onClick.AddListener(OnQuit);
    }

    private void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_howToPlayUI != null && _howToPlayUI.IsOpen)
        {
            _howToPlayUI.Close();
            return;
        }

        if (_creditsUI != null && _creditsUI.IsOpen)
        {
            _creditsUI.Close();
            return;
        }

        if (_optionsUI != null && _optionsUI.IsOpen)
        {
            _optionsUI.Close();
            return;
        }
    }

    private void OnGameStart()
    {
        SceneLoader.Instance?.LoadScene(_gameSceneName);
    }

    private void OnHowToPlay()
    {
        if (_optionsUI != null && _optionsUI.IsOpen) return; // 옵션 열려있으면 무시
        _howToPlayUI?.Open();
        SetMenuButtonsInteractable(false);
    }

    private void OnOptions()
    {
        if (_howToPlayUI != null && _howToPlayUI.IsOpen) return; // HowToPlay 열려있으면 무시
        _optionsUI?.Open();
        SetMenuButtonsInteractable(false);
    }

    private void OnCredits()
    {
        if (_howToPlayUI != null && _howToPlayUI.IsOpen) return;
        if (_optionsUI != null && _optionsUI.IsOpen) return;
        _creditsUI?.Open();
        SetMenuButtonsInteractable(false);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 패널 닫힐 때 버튼 다시 활성화
    public void OnPanelClosed()
    {
        SetMenuButtonsInteractable(true);
    }

    private void SetMenuButtonsInteractable(bool interactable)
    {
        if (_gameStartBtn) _gameStartBtn.interactable = interactable;
        if (_howToPlayBtn) _howToPlayBtn.interactable = interactable;
        if (_optionsBtn) _optionsBtn.interactable = interactable;
        if (_quitBtn) _quitBtn.interactable = interactable;
    }
}