using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _optionsPanel;

    [Header("타이틀 메뉴")]
    [SerializeField] private TitleMenuUI _titleMenuUI;

    [Header("탭 버튼")]
    [SerializeField] private Button _graphicTabBtn;
    [SerializeField] private Button _audioTabBtn;
    [SerializeField] private Button _controlTabBtn;

    [Header("탭 버튼 색상")]
    [SerializeField] private Color _selectedColor = Color.black;
    [SerializeField] private Color _deselectedColor = Color.white;

    [Header("설정 패널")]
    [SerializeField] private GameObject _graphicPanel;
    [SerializeField] private GameObject _audioPanel;
    [SerializeField] private GameObject _controlPanel;

    [Header("Graphic")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _displayModeDropdown;

    [Header("Audio")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _uiSlider;

    [Header("Control")]
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private Slider _crosshairR;
    [SerializeField] private Slider _crosshairG;
    [SerializeField] private Slider _crosshairB;
    [SerializeField] private Image _crosshairColorPreview;

    [Header("Debug Console")]
    [SerializeField] private Toggle _debugConsoleToggle;

    [Header("버튼")]
    [SerializeField] private Button _closeBtn;
    [SerializeField] private Button _backToTitleBtn;

    [Header("Refs")]
    [SerializeField] private PlayerInputHub _input;

    [Header("씬 이름")]
    [SerializeField] private string _titleSceneName = "Title";

    public bool IsOpen => _optionsPanel != null && _optionsPanel.activeSelf;

    public static bool IsOptionsOpen { get; set; }

    private bool _isIngame;
    private Image _graphicTabImage;
    private Image _audioTabImage;
    private Image _controlTabImage;

    private void Awake()
    {
        _isIngame = SceneManager.GetActiveScene().name != _titleSceneName;
        _backToTitleBtn?.gameObject.SetActive(_isIngame);

        // 탭 버튼 이미지 캐싱
        _graphicTabImage = _graphicTabBtn?.GetComponent<Image>();
        _audioTabImage = _audioTabBtn?.GetComponent<Image>();
        _controlTabImage = _controlTabBtn?.GetComponent<Image>();

        // 탭 버튼 이벤트
        _graphicTabBtn?.onClick.AddListener(() => SwitchTab(0));
        _audioTabBtn?.onClick.AddListener(() => SwitchTab(1));
        _controlTabBtn?.onClick.AddListener(() => SwitchTab(2));

        // 닫기/타이틀
        _closeBtn?.onClick.AddListener(Close);
        _backToTitleBtn?.onClick.AddListener(OnBackToTitle);

        // Graphic
        _resolutionDropdown?.onValueChanged.AddListener(OnResolutionChanged);
        _displayModeDropdown?.onValueChanged.AddListener(OnDisplayModeChanged);

        // Audio
        _masterSlider?.onValueChanged.AddListener(v => OptionsManager.Instance?.SetMasterVolume(v));
        _bgmSlider?.onValueChanged.AddListener(v => OptionsManager.Instance?.SetBgmVolume(v));
        _sfxSlider?.onValueChanged.AddListener(v => OptionsManager.Instance?.SetSfxVolume(v));
        _uiSlider?.onValueChanged.AddListener(v => OptionsManager.Instance?.SetUiVolume(v));

        // Control
        _sensitivitySlider?.onValueChanged.AddListener(v => OptionsManager.Instance?.SetMouseSensitivity(v));
        _crosshairR?.onValueChanged.AddListener(_ => OnCrosshairColorChanged());
        _crosshairG?.onValueChanged.AddListener(_ => OnCrosshairColorChanged());
        _crosshairB?.onValueChanged.AddListener(_ => OnCrosshairColorChanged());

        // DebugConsole
        _debugConsoleToggle?.onValueChanged.AddListener(OnDebugConsoleToggle);

        if (_isIngame && _input == null)
            _input = FindFirstObjectByType<PlayerInputHub>();

        // Display Mode 옵션 설정
        InitDisplayModeDropdown();
    }

    private void Start()
    {
        InitResolutionDropdown();
        RefreshUI();
        SwitchTab(0); // 항상 Graphic 탭으로 시작
    }

    private void Update()
    {
        if (!_isIngame) return;
        if (_input == null) return;

        if (_input.CancelPressedThisFrame && !_input.CancelConsumed)
        {
            if (ShopPanelUI.IsOpen) return;
            Toggle();
        }
    }

    // 탭 전환
    private void SwitchTab(int index)
    {
        _graphicPanel?.SetActive(index == 0);
        _audioPanel?.SetActive(index == 1);
        _controlPanel?.SetActive(index == 2);

        // 선택된 탭 버튼 검정, 나머지 흰색
        if (_graphicTabImage) _graphicTabImage.color = index == 0 ? _selectedColor : _deselectedColor;
        if (_audioTabImage) _audioTabImage.color = index == 1 ? _selectedColor : _deselectedColor;
        if (_controlTabImage) _controlTabImage.color = index == 2 ? _selectedColor : _deselectedColor;
    }

    // 열기/닫기
    public void Open()
    {
        IsOptionsOpen = true;
        _optionsPanel?.SetActive(true);
        SwitchTab(0);

        if (_isIngame)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SFXManager.Instance?.Pause();
        }
    }

    public void Close()
    {
        IsOptionsOpen = false;
        _optionsPanel?.SetActive(false);

        if (!_isIngame)
            _titleMenuUI?.OnPanelClosed();
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SFXManager.Instance?.Unpause();
        }
    }

    public void Toggle()
    {
        if (_optionsPanel != null && _optionsPanel.activeSelf)
            Close();
        else
            Open();
    }

    // BackToTitle
    private void OnBackToTitle()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        IsOptionsOpen = false;

        BgmManager.Instance?.Stop();
        SceneManager.LoadScene(_titleSceneName);
    }

    // Graphic
    private void InitResolutionDropdown()
    {
        if (_resolutionDropdown == null) return;
        _resolutionDropdown.ClearOptions();

        var resolutions = Screen.resolutions;
        var options = new System.Collections.Generic.List<string>();
        foreach (var r in resolutions)
            options.Add($"{r.width} x {r.height}");
        _resolutionDropdown.AddOptions(options);

        int saved = OptionsManager.Instance?.ResolutionIndex ?? resolutions.Length - 1;
        _resolutionDropdown.value = Mathf.Clamp(saved, 0, resolutions.Length - 1);
        _resolutionDropdown.RefreshShownValue();
    }

    private void InitDisplayModeDropdown()
    {
        if (_displayModeDropdown == null) return;
        _displayModeDropdown.ClearOptions();
        _displayModeDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "전체화면",
            "창모드 (테두리 없음)",
            "창모드"
        });
    }

    private void OnResolutionChanged(int index)
    {
        OptionsManager.Instance?.SetResolution(index);
    }

    private void OnDisplayModeChanged(int index)
    {
        FullScreenMode mode = index switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => FullScreenMode.ExclusiveFullScreen
        };
        OptionsManager.Instance?.SetDisplayMode(mode);
    }

    // Control
    private void OnCrosshairColorChanged()
    {
        if (_crosshairR == null) return;
        Color c = new Color(_crosshairR.value, _crosshairG.value, _crosshairB.value);
        OptionsManager.Instance?.SetCrosshairColor(c);
        if (_crosshairColorPreview != null)
            _crosshairColorPreview.color = c;
    }

    // DebugConsole
    private void OnDebugConsoleToggle(bool value)
    {
        DebugConsoleUI.Instance?.SetEnabled(value);
    }

    // UI 갱신
    private void RefreshUI()
    {
        if (OptionsManager.Instance == null) return;

        if (_masterSlider) _masterSlider.value = OptionsManager.Instance.MasterVolume;
        if (_bgmSlider) _bgmSlider.value = OptionsManager.Instance.BgmVolume;
        if (_sfxSlider) _sfxSlider.value = OptionsManager.Instance.SfxVolume;
        if (_uiSlider) _uiSlider.value = OptionsManager.Instance.UiVolume;
        if (_sensitivitySlider) _sensitivitySlider.value = OptionsManager.Instance.MouseSensitivity;

        Color c = OptionsManager.Instance.CrosshairColor;
        if (_crosshairR) _crosshairR.value = c.r;
        if (_crosshairG) _crosshairG.value = c.g;
        if (_crosshairB) _crosshairB.value = c.b;
        if (_crosshairColorPreview) _crosshairColorPreview.color = c;

        if (_displayModeDropdown)
            _displayModeDropdown.value = OptionsManager.Instance.DisplayModeIndex;

        if (_debugConsoleToggle && DebugConsoleUI.Instance != null)
            _debugConsoleToggle.isOn = DebugConsoleUI.Instance.gameObject.activeSelf;
    }
}