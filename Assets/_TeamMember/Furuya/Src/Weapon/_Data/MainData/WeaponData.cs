using UnityEngine;



[CreateAssetMenu(menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject, IWeaponInfo {
    public string weaponName;
    public WeaponType type;
    public int damage;
    public float cooldown;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed;

    [Header("Melee Settings")]
    [Tooltip("UŒ‚‚Ì”ÍˆÍ")]
    public float range;
    [Tooltip("‘O•ûUŒ‚”ÍˆÍ(”¼Œa)")]
    public float meleeAngle;

    [Header("Visual Effects")]
    public EffectType muzzleFlashType = EffectType.Default;
    public EffectType hitEffectType = EffectType.Default;

    public string WeaponName => weaponName;
}