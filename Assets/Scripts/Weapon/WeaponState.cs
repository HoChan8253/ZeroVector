using UnityEngine;

[System.Serializable]
public class WeaponState
{
    public int ammoInMag;
    public int reserveAmmo;

    public bool isReloading;
    public float reloadEndTime;

    public float nextFireTime;

    public float currentSpread;
    public float lastShotTime;

    public void ResetFromData(WeaponData data)
    {
        ammoInMag = Mathf.Clamp(data.startAmmoInMag, 0, data.magSize);
        reserveAmmo = Mathf.Max(0, data.startReserveAmmo);

        isReloading = false;
        reloadEndTime = 0f;

        nextFireTime = 0f;

        currentSpread = 0f;
        lastShotTime = -999f;
    }
}