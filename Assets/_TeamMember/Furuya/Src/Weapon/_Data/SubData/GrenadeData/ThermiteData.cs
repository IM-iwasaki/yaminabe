using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// テルミットグレネード
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Grenade/ThermiteGrenade")]
public class ThermiteData : GrenadeData {
    [Header("Settings")]
    public float duration;
}
