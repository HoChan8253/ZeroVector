using UnityEngine;

// FPS 마우스 시점 회전 스크립트
public class PlayerLook : MonoBehaviour
{
    [Header("Refs")]
    public Transform _cam;

    [Header("Settings")]
    public float _sensitivity = 0.08f;
    public float _pitchMin = -85f;
    public float _pitchMax = 85f;

    [Header("Recoil")]
    public float _recoilReturnSpeed = 2f;
    private float _recoilPitch; // 수직
    private float _recoilYaw; // 수평

    [Header("ADS")]
    public float _adsFOV = 40f;
    public float _normalFOV = 60f;
    public float _adsFOVSpeed = 10f;

    private PlayerInputHub _input;
    private float _pitch;
    private Camera _camera;

    private void Awake()
    {
        _input = GetComponent<PlayerInputHub>();
        _camera = _cam.GetComponent<Camera>();
    }

    private void Start()
    {
        // 마우스 화면 중앙에 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (ShopPanelUI.IsOpen) return;

        Vector2 look = _input.Look;

        // 좌우 입력은 플레이어 바디 회전
        float yawInput = look.x * _sensitivity;
        transform.Rotate(Vector3.up * yawInput);

        // 상하 입력은 카메라 상하
        _pitch -= look.y * _sensitivity;

        // 반동 복귀
        _recoilPitch = Mathf.Lerp(_recoilPitch, 0f, _recoilReturnSpeed * Time.deltaTime);
        _recoilYaw = Mathf.Lerp(_recoilYaw, 0f, _recoilReturnSpeed * Time.deltaTime);

        // 최종 카메라 회전 = pitch + 수직반동, yaw반동은 카메라 local Y에만
        float finalPitch = Mathf.Clamp(_pitch + _recoilPitch, _pitchMin, _pitchMax);

        _cam.localRotation = Quaternion.Euler(finalPitch, _recoilYaw, 0f);

        float targetFOV = _input.AimHeld ? _adsFOV : _normalFOV;
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, _adsFOVSpeed * Time.deltaTime);
    }

    public void AddRecoil(float pitchAmount, float yawAmount)
    {
        _recoilPitch -= pitchAmount;  // 위로 튀게
        _recoilYaw += yawAmount;    // 좌우 랜덤
    }
}