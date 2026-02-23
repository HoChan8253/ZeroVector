using UnityEngine;
using UnityEngine.InputSystem;

// FPS 마우스 시점 회전 스크립트
public class PlayerLook : MonoBehaviour
{
    [Header("Refs")]
    public Transform _cam;

    [Header("Settings")]
    public float _sensitivity = 0.08f;
    public float _pitchMin = -85f;
    public float _pitchMax = 85f;

    private PlayerControls _input;
    private Vector2 _look;
    private float _pitch;

    void Awake()
    {
        _input = new PlayerControls();
    }

    void OnEnable()
    {
        _input.Enable();
        _input.Player.Look.performed += OnLook;
        _input.Player.Look.canceled += OnLook;
    }

    void OnDisable()
    {
        _input.Player.Look.performed -= OnLook;
        _input.Player.Look.canceled -= OnLook;
        _input.Disable();
    }

    void Start()
    {
        // 마우스 화면 중앙에 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        _look = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        // 1) 좌우(yaw) : Player 회전
        float yaw = _look.x * _sensitivity;
        transform.Rotate(Vector3.up * yaw);

        // 2) 상하(pitch) : Camera 로컬 회전
        _pitch -= _look.y * _sensitivity;
        _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);

        _cam.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
