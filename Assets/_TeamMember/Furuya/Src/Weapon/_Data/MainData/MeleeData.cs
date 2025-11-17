using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ‹ßÚ•Šíƒf[ƒ^
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Weapons/MeleeData")]
public class MeleeData : WeaponData
{
    [Header("Melee Settings")]
    [Tooltip("UŒ‚‚Ì”ÍˆÍ")]
    public float range;
    [Tooltip("‘O•ûUŒ‚”ÍˆÍ(”¼Œa)")]
    public float meleeAngle;
}
