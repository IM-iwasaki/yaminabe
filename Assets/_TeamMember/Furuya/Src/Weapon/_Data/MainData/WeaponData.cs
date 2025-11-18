using UnityEngine;


/// <summary>
/// •Šíƒf[ƒ^
/// </summary>
public class WeaponData : ScriptableObject, IWeaponInfo {
    public string weaponName;
    public WeaponType type;
    public int damage;
    public float cooldown;

    [Header("Visual Effects")]
    public EffectType muzzleFlashType = EffectType.Default;
    public EffectType hitEffectType = EffectType.Default;

    public string WeaponName => weaponName;

    [Header("Gun Settings")]
    public int maxAmmo;
    public float reloadTime;
}