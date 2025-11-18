using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// èeÉfÅ[É^
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Weapons/GunData")]
public class GunData : WeaponData {
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed;


    [Header("Gun Settings")]
    public int maxAmmo;
    public float reloadTime;
    public float explosionRange;
}
