using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地雷データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Trap/LandMine")]
public class LandMineData : TrapData
{
    [Header("Explosion Settings")]
    public float explosionRadius = 3f;
    public bool canDamageAllies = false;
}