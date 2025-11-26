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

    public float explosionRange;

    public int multiShot = 1;
    public float burstDelay;

    [Header("Audio Effects")]
    public GunSEType se;
}