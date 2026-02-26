using UnityEngine;

public class WeaponAnimEvents : MonoBehaviour
{
    [SerializeField] private WeaponSwapManager _swap;

    private void Awake()
    {
        if (_swap == null) _swap = GetComponentInParent<WeaponSwapManager>();
    }

    public void AE_HolsterEnd()
    {
        _swap.OnHolsterEnd();
    }

    public void AE_EquipEnd()
    {
        _swap.OnEquipEnd();
    }
}