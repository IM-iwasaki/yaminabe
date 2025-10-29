using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SubWeapons/Trap/LandMine")]
public class LandMineData : TrapData
{
    [Header("Explosion Settings")]
    public float explosionRadius = 3f;
    public bool canDamageAllies = false;
}