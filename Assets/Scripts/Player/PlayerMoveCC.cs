using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveCC : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -20f;

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
        dir = transform.TransformDirection(dir) * moveSpeed;

        if (_cc.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += gravity * Time.deltaTime;

        dir.y = _yVel;

        _cc.Move(dir * Time.deltaTime);
    }
}
