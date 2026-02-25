using UnityEngine;
using UnityEngine.InputSystem;

// CharacterController 기반 이동 스크립트
public class PlayerMoveCC : MonoBehaviour
{
    [Header("Move")]
    public float _moveSpeed = 5f;
    public float _sprintMultiplier = 1.6f;
    public float _gravity = -20f;

    private CharacterController _cc;
    private PlayerInputHub _input;

    private float _yVel;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHub>();
    }

    void Update()
    {
        Vector2 move = _input.Move;

        bool isMoving = move.sqrMagnitude > 0.01f;
        bool isSprinting = _input.SprintHeld && isMoving;

        float speed = _moveSpeed * (isSprinting ? _sprintMultiplier : 1f);

        Vector3 dir = new Vector3(move.x, 0f, move.y);
        dir = transform.TransformDirection(dir) * speed;

        if (_cc.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += _gravity * Time.deltaTime; // 중력 적용

        dir.y = _yVel;

        // 이동 실행
        _cc.Move(dir * Time.deltaTime);
    }
}
