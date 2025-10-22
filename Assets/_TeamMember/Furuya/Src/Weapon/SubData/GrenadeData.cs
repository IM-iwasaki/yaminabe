using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "SubWeapons/GrenadeData")]
public class GrenadeData : SubWeaponData {

    GrenadeType grenadeType;

    [Header("Explosion Settings")]
    public float explosionRadius = 3f;
    public float explosionDelay = 1.5f;
    [Tooltip("チェックついてたら大丈夫")]
    public bool hasExplosion = true;

    [Header("Damage Settings")]
    [Tooltip("味方にもダメージを与えるか")]
    public bool canDamageAllies = false;

    [Header("Flag X")]
    [Tooltip("フラグでは使用しない")]
    public float duration;
}