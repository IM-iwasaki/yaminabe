using UnityEngine;


/// <summary>
/// 魔法武器データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Weapons/MainMagicData")]
public class MainMagicData : WeaponData {

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed;

    [Header("Magic Settings")]
    public int MPCost;
    public EffectType chargeEffectType;
    public float chargeTime = 0f;

    [Header("Projectile Settings")]
    public ProjectileType magicType = ProjectileType.Linear;
    public float initialHeightSpeed = 5f;

    [Header("Audio Effects")]
    public MagicSEType se;
}