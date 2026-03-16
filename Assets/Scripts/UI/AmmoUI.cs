using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public GunController _gun;
    public TMP_Text _ammoText;
    public TMP_Text _currentWeaponText;

    private void OnEnable()
    {
        if (_gun != null) _gun.OnAmmoChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (_gun != null) _gun.OnAmmoChanged -= UpdateUI;
    }

    public void UpdateWeaponName()
    {
        if (_currentWeaponText == null) return;
        _currentWeaponText.text = (_gun != null && _gun._data != null)
            ? _gun._data.weaponName
            : "";
    }

    private void UpdateUI(int mag, int reserve)
    {
        if (_gun == null || _ammoText == null) return;
        string reserveText = reserve.ToString();
        if (_gun._data != null && _gun._data.infiniteReserveAmmo)
            reserveText = "∞";
        _ammoText.text = $"{mag} / {reserveText}";
        _ammoText.color = (mag == 0) ? Color.red : Color.white;
    }
}