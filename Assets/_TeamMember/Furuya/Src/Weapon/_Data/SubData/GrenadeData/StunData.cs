using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタングレネード
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Grenade/StunGrenade")]
public class StunData : GrenadeData
{
    [Header("Settings")]
    public float duration;
}
