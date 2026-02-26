using UnityEngine;

public enum FireMode
{
    SemiAuto,
    FullAuto
}

[CreateAssetMenu(menuName = "FPS/Weapon Data", fileName = "WD_NewWeapon")]

public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "AR";

    [Header("Fire Mode")]
    public FireMode fireMode = FireMode.FullAuto;

    [Header("Gun Settings")]
    public float fireRate = 10f;
    public float range = 100f;
    public float damage = 25f;

    [Header("Ammo")]
    public int magSize = 30;
    public int startAmmoInMag = 30;
    public int startReserveAmmo = 90;

    [Header("Reload")]
    public float reloadDuration = 4.583f;

    [Header("Recoil")]
    public float recoilPitchAmount = 2f;
    public float recoilYawAmount = 1f;

    [Header("Spread")]
    public float spreadAmount = 0.02f;
    public float maxSpread = 0.08f;
    public float spreadRecoverSpeed = 8f;
    public float tapResetTime = 0.15f;

    [Header("Animation")]
    public float moveThreshold = 0.1f;

    [Header("Animator Override")]
    public AnimatorOverrideController animatorOverride;
}