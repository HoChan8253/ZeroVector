using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DebugConsoleUI : MonoBehaviour
{
    public static DebugConsoleUI Instance { get; private set; }

    [Header("패널")]
    [SerializeField] private GameObject _consolePanel;

    [Header("로그 영역")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private Transform _logContainer; // Vertical Layout Group 붙은 Content
    [SerializeField] private GameObject _logLinePrefab; // TMP_Text 하나짜리 prefab

    [Header("입력")]
    [SerializeField] private TMP_InputField _inputField;

    [Header("색상")]
    [SerializeField] private Color _colorLog = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color _colorWarning = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color _colorError = new Color(1f, 0.35f, 0.35f);

    [Header("옵션")]
    [SerializeField] private int _maxLines = 200;

    [Header("씬 이름")]
    [SerializeField] private string _titleSceneName = "Title";

    public static bool IsOpen { get; private set; }

    private bool _isIngame;

    private readonly Queue<GameObject> _linePool = new();
    private readonly LinkedList<GameObject> _activeLines = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_inputField != null)
            _inputField.onSubmit.AddListener(_ => OnSubmit());

        _consolePanel?.SetActive(false);
        IsOpen = false;

        UpdateIsIngame();
    }

    private void OnEnable()
    {
        Application.logMessageReceived += OnLogReceived;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= OnLogReceived;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateIsIngame();
        if (IsOpen) Close();
    }

    private void UpdateIsIngame()
    {
        _isIngame = SceneManager.GetActiveScene().name != _titleSceneName;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            Toggle();
    }

    public void Open()
    {
        IsOpen = true;
        _consolePanel?.SetActive(true);
        ScrollToBottom();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputField?.ActivateInputField();
    }

    public void Close()
    {
        IsOpen = false;
        _consolePanel?.SetActive(false);

        if (_isIngame && !OptionsUI.IsOptionsOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    // 로그 수신 (Application.logMessageReceived)
    private void OnLogReceived(string message, string stackTrace, LogType type)
    {
        Color color = type switch
        {
            LogType.Warning => _colorWarning,
            LogType.Error or LogType.Exception or LogType.Assert => _colorError,
            _ => _colorLog,
        };

        string prefix = type switch
        {
            LogType.Warning => "[WARN] ",
            LogType.Error or LogType.Exception or LogType.Assert => "[ERR]  ",
            _ => "[LOG]  ",
        };

        AddLine(prefix + message, color);
    }

    public void AddLine(string text, Color color)
    {
        while (_activeLines.Count >= _maxLines)
        {
            var oldest = _activeLines.First.Value;
            _activeLines.RemoveFirst();
            oldest.SetActive(false);
            _linePool.Enqueue(oldest);
        }

        GameObject lineObj;
        if (_linePool.Count > 0)
        {
            lineObj = _linePool.Dequeue();
            lineObj.SetActive(true);
        }
        else
        {
            lineObj = Instantiate(_logLinePrefab, _logContainer);
        }

        var tmp = lineObj.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        lineObj.transform.SetAsLastSibling();
        _activeLines.AddLast(lineObj);

        if (IsOpen)
            ScrollToBottom();
    }

    public void AddLine(string text) => AddLine(text, _colorLog);

    // 명령어 제출
    private void OnSubmit()
    {
        if (_inputField == null) return;
        string cmd = _inputField.text.Trim();
        _inputField.text = string.Empty;
        _inputField.ActivateInputField();

        if (string.IsNullOrEmpty(cmd)) return;

        AddLine("> " + cmd, new Color(0.6f, 1f, 0.6f));
        DebugConsoleCommands.Execute(cmd);
    }

    private void ScrollToBottom()
    {
        if (_scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
    }

    //OptionsUI Debug Console 체크박스 onValueChanged 에 연결
    public void SetEnabled(bool enable)
    {
        if (!enable && IsOpen) Close();
        gameObject.SetActive(enable);
    }

    //DebugConsoleCommands clear 명령용
    public Transform GetLogContainer() => _logContainer;
}