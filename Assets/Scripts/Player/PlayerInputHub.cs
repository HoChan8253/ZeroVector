using System.Security.Claims;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHub : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public bool FireHeld { get; private set; }

    public bool AimHeld { get; private set; }
    public bool FirePressedThisFrame { get; private set; }
    public bool ReloadPressedThisFrame { get; private set; }
    public bool Weapon1PressedThisFrame { get; private set; }
    public bool Weapon2PressedThisFrame { get; private set; }
    public bool Weapon3PressedThisFrame { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool ShopPressedThisFrame { get; private set; }
    public bool CancelPressedThisFrame { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }

    // 테스트용
    public bool ToggleDayNightPressedThisFrame { get; private set; }

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
        _input.Player.Aim.performed += OnAim;
        _input.Player.Aim.canceled += OnAim;
        _input.Player.Reload.performed += OnReload;
        _input.Player.Sprint.performed += OnSprint;
        _input.Player.Sprint.canceled += OnSprint;
        _input.Player.Weapon1.performed += OnWeapon1;
        _input.Player.Weapon2.performed += OnWeapon2;
        _input.Player.Weapon3.performed += OnWeapon3;
        _input.Player.Shop.performed += OnShop;
        _input.Menu.Cancel.performed += OnCancel;
        _input.Player.Jump.performed += OnJump;
        _input.Menu.Enable();

        // 테스트용
        _input.Debug.ToggleDayNight.performed += OnToggleDayNight;
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        _input.Player.Look.performed -= OnLook;
        _input.Player.Look.canceled -= OnLook;
        _input.Player.Fire.performed -= OnFire;
        _input.Player.Fire.canceled -= OnFire;
        _input.Player.Aim.performed -= OnAim;
        _input.Player.Aim.canceled -= OnAim;
        _input.Player.Reload.performed -= OnReload;
        _input.Player.Sprint.performed -= OnSprint;
        _input.Player.Sprint.canceled -= OnSprint;
        _input.Player.Weapon1.performed -= OnWeapon1;
        _input.Player.Weapon2.performed -= OnWeapon2;
        _input.Player.Weapon3.performed -= OnWeapon3;
        _input.Player.Shop.performed -= OnShop;
        _input.Menu.Cancel.performed -= OnCancel;
        _input.Player.Jump.performed -= OnJump;
        _input.Menu.Disable();

        // 테스트용
        _input.Debug.ToggleDayNight.performed -= OnToggleDayNight;

        _input.Disable();
    }

    private void LateUpdate()
    {
        FirePressedThisFrame = false;
        ReloadPressedThisFrame = false;
        Weapon1PressedThisFrame = false;
        Weapon2PressedThisFrame = false;
        Weapon3PressedThisFrame = false;
        ShopPressedThisFrame = false;
        CancelPressedThisFrame = false;
        JumpPressedThisFrame = false;

        // 테스트용
        ToggleDayNightPressedThisFrame = false;
    }

    private void OnMove(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();
    private void OnLook(InputAction.CallbackContext ctx) => Look = ctx.ReadValue<Vector2>();

    private void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { FireHeld = true; FirePressedThisFrame = true; }
        if (ctx.canceled) FireHeld = false;
    }

    private void OnAim(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) AimHeld = true;
        if (ctx.canceled) AimHeld = false;
    }

    private void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ReloadPressedThisFrame = true;
    }

    private void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) SprintHeld = true;
        if (ctx.canceled) SprintHeld = false;
    }

    private void OnWeapon1(InputAction.CallbackContext ctx) { if (ctx.performed) Weapon1PressedThisFrame = true; }
    private void OnWeapon2(InputAction.CallbackContext ctx) { if (ctx.performed) Weapon2PressedThisFrame = true; }
    private void OnWeapon3(InputAction.CallbackContext ctx) { if (ctx.performed) Weapon3PressedThisFrame = true; }

    private void OnShop(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ShopPressedThisFrame = true;
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) CancelPressedThisFrame = true;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) JumpPressedThisFrame = true;
    }

    // 테스트용
    private void OnToggleDayNight(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) ToggleDayNightPressedThisFrame = true;
    }
}