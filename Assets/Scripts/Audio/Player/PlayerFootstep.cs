using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float _walkStepInterval = 0.5f;
    [SerializeField] private float _runStepInterval = 0.3f;

    private PlayerMoveCC _move;
    private PlayerInputHub _input;
    private PlayerStats _stats;

    private float _stepTimer;

    private void Awake()
    {
        _move = GetComponent<PlayerMoveCC>();
        _input = GetComponent<PlayerInputHub>();
        _stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!_move.IsGrounded) return;
        if (_input.Move.sqrMagnitude < 0.01f) return;

        bool isSprinting = _input.SprintHeld && _stats.CanSprint;
        float interval = isSprinting ? _runStepInterval : _walkStepInterval;

        _stepTimer -= Time.deltaTime;
        if (_stepTimer > 0f) return;

        _stepTimer = interval;

        SFXManager.PlaySound(SoundType.PlayerFootstep);
    }
}