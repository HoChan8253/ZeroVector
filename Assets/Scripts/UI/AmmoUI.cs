using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public GunController _gun;
    public TMP_Text _ammoText;

    private void OnEnable()
    {
        _gun.OnAmmoChanged += UpdateUI;
    }

    private void OnDisable()
    {
        _gun.OnAmmoChanged -= UpdateUI;
    }

    private void UpdateUI(int mag, int reserve)
    {
        _ammoText.text = $"{mag} / {reserve}";

        if (mag == 0)
            _ammoText.color = Color.red;
        else
            _ammoText.color = Color.white;
    }
}