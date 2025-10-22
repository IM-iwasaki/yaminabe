using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SubWeapons/Grenade/StunGrenade")]
public class StunData : GrenadeData
{
    [Header("Settings")]
    public float duration;
}
