using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フラッシュグレネード
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Grenade/FlashGrenade")]
public class FlashData : GrenadeData
{
    [Header("Settings")]
    public float duration;
}
