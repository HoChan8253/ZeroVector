using UnityEngine;

public class WeaponSwapManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInputHub _input;
    [SerializeField] private GunController _gun;

    [Header("Loadout (1~3)")]
    [SerializeField] private WeaponData _slot1;
    [SerializeField] private WeaponData _slot2;
    [SerializeField] private WeaponData _slot3;

    private WeaponData _pending;

    private void Awake()
    {
        if (_input == null) _input = GetComponentInParent<PlayerInputHub>();
        if (_gun == null) _gun = GetComponentInChildren<GunController>();
    }

    private void Update()
    {
        if (_gun == null || _input == null) return;

        if (_input.Weapon1PressedThisFrame) RequestSwap(_slot1);
        if (_input.Weapon2PressedThisFrame) RequestSwap(_slot2);
        if (_input.Weapon3PressedThisFrame) RequestSwap(_slot3);
    }

    private void RequestSwap(WeaponData target)
    {
        if (target == null) return;
        if (_gun.IsBusy) return;
        if (_gun._data == target) return; // 같은 무기면 무시

        _pending = target;
        _gun.BeginHolsterForSwap();
    }

    // ⭐ Holster 애니 끝에서 호출(Animation Event)
    public void OnHolsterEnded()
    {
        if (_pending == null) return;

        _gun.ForceEquipData(_pending);
        _pending = null;
    }
}