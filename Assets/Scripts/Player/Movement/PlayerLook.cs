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

    [Header("Recoil")]
    public float _recoilReturnSpeed = 2f;
    private float _recoilOffset;

    private PlayerInputHub _input;
    private float _pitch;

    void Awake()
    {
        _input = GetComponent<PlayerInputHub>();
    }

    void Start()
    {
        // 마우스 화면 중앙에 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 _look = _input.Look; 

        // 좌우(yaw) : Player 회전
        float yaw = _look.x * _sensitivity;
        transform.Rotate(Vector3.up * yaw);
        
        // 마우스 입력 반영
        _pitch -= _look.y * _sensitivity;

        // 반동 복귀 처리 (부드럽게 0으로)
        _recoilOffset = Mathf.Lerp(_recoilOffset, 0f, _recoilReturnSpeed * Time.deltaTime);

        // 최종 pitch에 반동 더하기
        float finalPitch = _pitch + _recoilOffset;

        finalPitch = Mathf.Clamp(finalPitch, _pitchMin, _pitchMax);

        _cam.localRotation = Quaternion.Euler(finalPitch, 0f, 0f);
    }

    public void AddRecoil(float amount)
    {
        _recoilOffset -= amount;
    }
}
