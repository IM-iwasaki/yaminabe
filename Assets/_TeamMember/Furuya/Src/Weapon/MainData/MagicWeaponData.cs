using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Weapons/MagicWeaponData")]
public class MagicWeaponData : WeaponData {
    [Header("Magic Settings")]
    public ParticleSystem chargeEffect;
    public float chargeTime = 1.0f;

    [Header("Projectile Settings")]
    public ProjectileType magicType = ProjectileType.Linear;
    public float initialHeightSpeed = 5f;
}
