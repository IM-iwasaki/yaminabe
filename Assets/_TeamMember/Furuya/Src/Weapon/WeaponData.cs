using UnityEngine;

public enum WeaponType { Melee, Gun, Magic}

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
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
}
