using UnityEngine;
using UnityEngine.InputSystem;

// CharacterController 기반 이동 스크립트
public class PlayerMoveCC : MonoBehaviour
{
    public float _moveSpeed = 5f;
    public float _gravity = -20f;

    private CharacterController _cc;
    private PlayerControls _input;
    private Vector2 _move;
    private float _yVel;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = new PlayerControls();
    }

    void OnEnable()
    {
        _input.Enable();
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        _input.Disable();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        _move = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        Vector3 dir = new Vector3(_move.x, 0f, _move.y);
        dir = transform.TransformDirection(dir) * _moveSpeed;

        if (_cc.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += _gravity * Time.deltaTime; // 중력 적용

        dir.y = _yVel;

        // 이동 실행
        _cc.Move(dir * Time.deltaTime);
    }
}
