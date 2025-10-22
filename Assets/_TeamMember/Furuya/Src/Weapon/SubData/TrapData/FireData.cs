using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SubWeapons/Trap/FireTrap")]
public class FireData : TrapData
{
    [Header("Fire Settings")]
    public float damagePerSecond = 5f;
    public float effectRadius = 2f;
    public bool canDamageAllies = false;
}
