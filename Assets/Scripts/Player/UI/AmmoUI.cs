using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public GunController _gun;
    public TMP_Text _ammoText;
    public TMP_Text _reserveText;

    private void OnEnable()
    {
        _gun.OnAmmoChanged += UpdateUI;
    }

    private void OnDisable()
    {
        _gun.OnAmmoChanged -= UpdateUI;
    }

    private void UpdateUI(int ammoInMag, int reserveAmmo)
    {
        _ammoText.text = ammoInMag.ToString();
        _reserveText.text = reserveAmmo.ToString();
    }
}