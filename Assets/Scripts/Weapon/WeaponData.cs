using UnityEngine;

public enum FireMode
{
    SemiAuto,
    FullAuto
}

public enum ReloadType
{
    Magazine,    // 한번에 채움
    PerShell     // 한 발씩
}

[CreateAssetMenu(menuName = "WeaponSO/Weapon Data", fileName = "WD_NewWeapon")]

public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "AR";

    [Header("Fire Mode")]
    public FireMode fireMode = FireMode.FullAuto;

    [Header("Shotgun")]
    public int pelletCount = 8;
    public float pelletSpread = 0.08f;
    public float damagePerPellet = 8f;

    [Header("Gun Settings")]
    public float fireRate = 10f;
    public float range = 100f;
    public float damage = 25f;

    [Header("Ammo")]
    public int magSize = 30;
    public int startAmmoInMag = 30;
    public int startReserveAmmo = 90;

    [Header("Ammo Option")]
    public bool infiniteReserveAmmo = false;

    [Header("Reload")]
    public float reloadDuration = 4.583f;

    [Header("Reload Type")]
    public ReloadType reloadType = ReloadType.Magazine;

    // 한 발 장전 시간
    public float perShellTime = 1.667f;

    // Shotgun 전용 (장전 시작 / 종료 애니 길이)
    public float reloadStartTime = 1.0f; // IdleToReload 길이
    public float reloadEndTime = 0.917f; // ReloadToIdle 길이

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