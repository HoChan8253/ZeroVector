using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHub : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool FireHeld { get; private set; }
    public bool FirePressedThisFrame { get; private set; }
    public bool ReloadPressedThisFrame { get; private set; }

    private PlayerControls _input;

    private void Awake()
    {
        _input = new PlayerControls();
    }

    private void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;

        _input.Player.Look.performed += OnLook;
        _input.Player.Look.canceled += OnLook;

        _input.Player.Fire.performed += OnFire;
        _input.Player.Fire.canceled += OnFire;

        _input.Player.Reload.performed += OnReload;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;

        _input.Player.Look.performed -= OnLook;
        _input.Player.Look.canceled -= OnLook;

        _input.Player.Fire.performed -= OnFire;
        _input.Player.Fire.canceled -= OnFire;

        _input.Player.Reload.performed -= OnReload;

        _input.Disable();
    }

    private void LateUpdate()
    {
        FirePressedThisFrame = false;
        ReloadPressedThisFrame = false;
    }

    private void OnMove(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();
    private void OnLook(InputAction.CallbackContext ctx) => Look = ctx.ReadValue<Vector2>();

    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            FireHeld = true;
            FirePressedThisFrame = true;
        }

        if (ctx.canceled)
            FireHeld = false;
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ReloadPressedThisFrame = true;
    }
}