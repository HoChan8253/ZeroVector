using UnityEngine;
using UnityEngine.EventSystems;

// CharacterController 기반 이동 스크립트
public class PlayerMoveCC : MonoBehaviour
{
    [Header("Move")]
    public float _moveSpeed = 5f;
    public float _sprintMultiplier = 1.6f;
    public float _gravity = -20f;

    [Header("Wall SphereCast")]
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private float _checkRadius = 0.35f;
    [SerializeField] private float _checkDistance = 0.6f;
    [SerializeField] private float _checkHeight = 0.9f;
    [SerializeField] private float _intoWallDotThreshold = 0.05f;

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

        bool wantsSprint = _input.SprintHeld && isMoving;
        bool canSprint = _stats.CanSprint;

        bool isSprinting = wantsSprint && canSprint;

        _stats.TickStamina(isSprinting);

        float speed = _moveSpeed * (isSprinting ? _sprintMultiplier : 1f);

        Vector3 moveDirection = new Vector3(move.x, 0f, move.y);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        // 벽쪽으로 이동 불가
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Vector3 origin = transform.position + Vector3.up * _checkHeight;

            // 이동 방향 기준으로 검사
            Vector3 dir = moveDirection.normalized;

            if (Physics.SphereCast(origin, _checkRadius, dir, out RaycastHit hit, _checkDistance, _wallMask, QueryTriggerInteraction.Ignore))
            {
                // 벽 안쪽으로 밀고 있는지 확인
                float intoWall = Vector3.Dot(dir, -hit.normal);

                if (intoWall > _intoWallDotThreshold)
                {
                    // 벽을 따라 미끄러짐
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
            }
        }

        if (_cc.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += _gravity * Time.deltaTime;

        Vector3 dirFinal = moveDirection;
        dirFinal.y = _yVel;

        _cc.Move(dirFinal * Time.deltaTime);
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