using UnityEngine;


/// <summary>
/// 武器データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Weapons/WeaponData")]
public class WeaponData : ScriptableObject, IWeaponInfo {
    public string weaponName;
    public WeaponType type;
    public int damage;
    public float cooldown;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed;

    [Header("Melee Settings")]
    [Tooltip("攻撃の範囲")]
    public float range;
    [Tooltip("前方攻撃範囲(半径)")]
    public float meleeAngle;

    [Header("Visual Effects")]
    public EffectType muzzleFlashType = EffectType.Default;
    public EffectType hitEffectType = EffectType.Default;

    public string WeaponName => weaponName;
}