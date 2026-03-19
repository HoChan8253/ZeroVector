using UnityEngine;
using UnityEngine.UI;

public class TitleMenuUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _gameStartBtn;
    [SerializeField] private Button _howToPlayBtn;
    [SerializeField] private Button _optionsBtn;
    [SerializeField] private Button _quitBtn;

    [Header("패널")]
    [SerializeField] private GameObject _howToPlayPanel;
    [SerializeField] private Button _howToPlayCloseBtn;
    [SerializeField] private OptionsUI _optionsUI;

    [Header("씬 이름")]
    [SerializeField] private string _gameSceneName = "InGame";

    private void Awake()
    {
        _gameStartBtn?.onClick.AddListener(OnGameStart);
        _howToPlayBtn?.onClick.AddListener(OnHowToPlay);
        _optionsBtn?.onClick.AddListener(OnOptions);
        _quitBtn?.onClick.AddListener(OnQuit);
        _howToPlayCloseBtn?.onClick.AddListener(() => _howToPlayPanel?.SetActive(false));

        _howToPlayPanel?.SetActive(false);
    }

    private void OnGameStart()
    {
        SceneLoader.Instance?.LoadScene(_gameSceneName);
    }

    private void OnHowToPlay()
    {
        _howToPlayPanel?.SetActive(true);
    }

    private void OnOptions()
    {
        _optionsUI?.Open();
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}