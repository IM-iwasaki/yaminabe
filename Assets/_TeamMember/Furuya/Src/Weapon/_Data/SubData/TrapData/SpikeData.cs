using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 針データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Trap/SpikeTrap")]
public class SpikeData : TrapData {

    [Header("Damage Settings")]
    [Tooltip("味方にもダメージを与えるか")]
    public bool canDamageAllies = false;
}
