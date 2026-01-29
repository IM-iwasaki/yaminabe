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


    //霜踏み専用
    [Header("For Skills")]
    public int stepCount = 6;
    public float stepDistance = 1.2f;
    public float stepInterval = 0.08f;
    public float hitboxLifeTime = 0.25f;
}