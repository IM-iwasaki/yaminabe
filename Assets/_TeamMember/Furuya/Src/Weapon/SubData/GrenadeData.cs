using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "SubWeapons/GrenadeData")]
public class GrenadeData : SubWeaponData {

    GrenadeType grenadeType;

    [Header("Explosion Settings")]
    public float explosionRadius = 3f;
    public float explosionDelay = 1.5f;
    public bool hasExplosion = true;

    [Header("Damage Settings")]
    [Tooltip("–¡•û‚É‚àƒ_ƒ[ƒW‚ğ—^‚¦‚é‚©")]
    public bool canDamageAllies = false;

    [Header("Flag X")]
    public float duration;
}