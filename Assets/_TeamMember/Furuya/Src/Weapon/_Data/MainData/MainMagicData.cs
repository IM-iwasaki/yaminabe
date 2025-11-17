using UnityEngine;


/// <summary>
/// 魔法武器データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Weapons/MainMagicData")]
public class MainMagicData : WeaponData {
    [Header("Magic Settings")]
    public ParticleSystem chargeEffect;
    public float chargeTime = 1.0f;

    [Header("Projectile Settings")]
    public ProjectileType magicType = ProjectileType.Linear;
    public float initialHeightSpeed = 5f;
}