using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スモークグレ
/// </summary>

[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Grenade/SmokeGrenade")]
public class SmokeData : GrenadeData
{
    [Header("Settings")]
    public float duration;
}
