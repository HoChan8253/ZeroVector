using UnityEngine;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer _audioMixer;

    // 설정값
    public float BgmVolume { get; private set; } = 0.1f;
    public float SfxVolume { get; private set; } = 1f;
    public float MouseSensitivity { get; private set; } = 0.08f;
    public Color CrosshairColor { get; private set; } = Color.white;
    public int ResolutionIndex { get; private set; } = 0;
    public bool IsFullscreen { get; private set; } = true;
    public float MasterVolume { get; private set; } = 1f;
    public float UiVolume { get; private set; } = 1f;
    public int DisplayModeIndex { get; private set; } = 0;

    public event System.Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Start()
    {
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        SetMasterVolume(MasterVolume);
        SetBgmVolume(BgmVolume);
        SetSfxVolume(SfxVolume);
        SetUiVolume(UiVolume);
    }

    // Master Volume
    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        _audioMixer?.SetFloat("Master", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat("Master", value);
        OnSettingsChanged?.Invoke();
    }

    // BGM Volume
    public void SetBgmVolume(float value)
    {
        BgmVolume = value;
        _audioMixer?.SetFloat("BGM", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat("BGM", value);
        OnSettingsChanged?.Invoke();
    }

    // SFX Volume
    public void SetSfxVolume(float value)
    {
        SfxVolume = value;
        _audioMixer?.SetFloat("SFX", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat("SFX", value);
        OnSettingsChanged?.Invoke();
    }

    // UI Volume
    public void SetUiVolume(float value)
    {
        UiVolume = value;
        _audioMixer?.SetFloat("UI", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat("UI", value);
        OnSettingsChanged?.Invoke();
    }

    // 마우스 감도
    public void SetMouseSensitivity(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);

        var look = FindFirstObjectByType<PlayerLook>();
        if (look != null) look._sensitivity = value;

        OnSettingsChanged?.Invoke();
    }

    // 크로스헤어 색상
    public void SetCrosshairColor(Color color)
    {
        CrosshairColor = color;
        PlayerPrefs.SetFloat("CrosshairR", color.r);
        PlayerPrefs.SetFloat("CrosshairG", color.g);
        PlayerPrefs.SetFloat("CrosshairB", color.b);
        OnSettingsChanged?.Invoke();
    }

    // 해상도
    public void SetResolution(int index, int width, int height)
    {
        ResolutionIndex = index;
        Screen.SetResolution(width, height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        OnSettingsChanged?.Invoke();
    }

    // 전체화면
    public void SetFullscreen(bool isFullscreen)
    {
        IsFullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        OnSettingsChanged?.Invoke();
    }

    public void SetDisplayMode(FullScreenMode mode)
    {
        DisplayModeIndex = mode switch
        {
            FullScreenMode.ExclusiveFullScreen => 0,
            FullScreenMode.FullScreenWindow => 1,
            FullScreenMode.Windowed => 2,
            _ => 0
        };
        Screen.fullScreenMode = mode;
        PlayerPrefs.SetInt("DisplayMode", DisplayModeIndex);
        OnSettingsChanged?.Invoke();
    }

    // 저장된 설정 불러오기
    private void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat("Master", 1f);
        BgmVolume = PlayerPrefs.GetFloat("BGM", 0.1f);
        SfxVolume = PlayerPrefs.GetFloat("SFX", 1f);
        UiVolume = PlayerPrefs.GetFloat("UI", 1f);
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 0.08f);

        int defaultResIndex = 0;
        var resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
            {
                defaultResIndex = i;
                break;
            }
        }
        ResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", defaultResIndex);
        DisplayModeIndex = PlayerPrefs.GetInt("DisplayMode", 0);
        IsFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        CrosshairColor = new Color(
            PlayerPrefs.GetFloat("CrosshairR", 1f),
            PlayerPrefs.GetFloat("CrosshairG", 1f),
            PlayerPrefs.GetFloat("CrosshairB", 1f));

        var res = Screen.resolutions;
        if (ResolutionIndex >= 0 && ResolutionIndex < res.Length)
            Screen.SetResolution(res[ResolutionIndex].width, res[ResolutionIndex].height, Screen.fullScreenMode);
    }
}