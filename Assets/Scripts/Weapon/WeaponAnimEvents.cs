using UnityEngine;

public class WeaponAnimEvents : MonoBehaviour
{
    [SerializeField] private WeaponSwapManager _swap;
    [SerializeField] private GunController _gun;

    private void Awake()
    {
        if (_swap == null) _swap = GetComponentInParent<WeaponSwapManager>();
        if (_gun == null) _gun = GetComponentInParent<GunController>();
    }

    public void AE_HolsterEnd()
    {
        _swap.OnHolsterEnd();
    }

    public void AE_EquipEnd()
    {
        _swap.OnEquipEnd();
    }

    public void AE_PumpEnd()
    {
        if (_gun != null) _gun.OnPumpEnd();
    }
}