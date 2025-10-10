using UnityEngine;

public enum WeaponType { Melee, Gun, Magic }

public enum EffectType { Default, Fire, Ice, Lightning, Explosion }

[CreateAssetMenu(menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject {
    public string weaponName;
    public WeaponType type;
    public int damage;
    public float range;
    public float cooldown;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed;

    [Header("Visual Effects")]
    public EffectType muzzleFlashType = EffectType.Default;
    public EffectType hitEffectType = EffectType.Default;
}
