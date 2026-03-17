using UnityEngine;
using UnityEngine.EventSystems;

// CharacterController 기반 이동 스크립트
public class PlayerMoveCC : MonoBehaviour
{
    [Header("Move")]
    public float _moveSpeed = 5f;
    public float _sprintMultiplier = 1.6f;
    public float _gravity = -20f;

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _walkJumpForwardForce = 2f;   // 걷기 중 점프 전진력
    [SerializeField] private float _sprintJumpForwardForce = 4f; // 달리기 중 점프 전진력
    [SerializeField] private float _jumpStaminaCost = 10f;

    public bool IsGrounded => _cc.isGrounded;

    private bool _isJumping;

    [Header("ADS")]
    [SerializeField] private float _adsMoveMultiplier = 0.5f;

    [Header("Wall SphereCast")]
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private float _checkRadius = 0.35f;
    [SerializeField] private float _checkDistance = 0.6f;
    [SerializeField] private float _checkHeight = 0.9f;
    [SerializeField] private float _intoWallDotThreshold = 0.05f;

    private Vector3 _knockbackVelocity;
    private float _knockbackDecay = 8f;

    private CharacterController _cc;
    private PlayerInputHub _input;
    private PlayerStats _stats;

    private float _yVel;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputHub>();
        _stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        Vector2 move = _input.Move;
        bool isMoving = move.sqrMagnitude > 0.01f;
        bool isAiming = _input.AimHeld;
        bool wantsSprint = _input.SprintHeld && isMoving && !isAiming;
        bool canSprint = _stats.CanSprint;
        bool isSprinting = wantsSprint && canSprint;
        _stats.TickStamina(isSprinting);
        float speed = _moveSpeed;
        if (isSprinting) speed *= _sprintMultiplier;
        if (isAiming) speed *= _adsMoveMultiplier;

        Vector3 moveDirection = new Vector3(move.x, 0f, move.y);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        // 벽쪽으로 이동 불가
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Vector3 origin = transform.position + Vector3.up * _checkHeight;
            Vector3 dir = moveDirection.normalized;
            if (Physics.SphereCast(origin, _checkRadius, dir, out RaycastHit hit, _checkDistance, _wallMask, QueryTriggerInteraction.Ignore))
            {
                float intoWall = Vector3.Dot(dir, -hit.normal);
                if (intoWall > _intoWallDotThreshold)
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
            }
        }

        if (_cc.isGrounded && _yVel < 0f) _yVel = -2f;

        // ★ 점프
        if (_cc.isGrounded && _input.JumpPressedThisFrame)
        {
            _yVel = Mathf.Sqrt(-2f * _gravity * _jumpHeight);
            _stats.ConsumeStamina(_jumpStaminaCost);

            // 이동 중 점프 시 전진력 추가
            if (isMoving)
            {
                float forwardForce = isSprinting ? _sprintJumpForwardForce : _walkJumpForwardForce;
                _knockbackVelocity += moveDirection.normalized * forwardForce;
            }
        }

        _yVel += _gravity * Time.deltaTime;

        Vector3 dirFinal = moveDirection + _knockbackVelocity;
        dirFinal.y = _yVel;
        _cc.Move(dirFinal * Time.deltaTime);
        _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, _knockbackDecay * Time.deltaTime);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        _knockbackVelocity = direction.normalized * force;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 현재 기준 위치에 구를 그려서 감지 위치/반경 확인
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * _checkHeight;
        Gizmos.DrawWireSphere(origin, _checkRadius);
    }
#endif
}