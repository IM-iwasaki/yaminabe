using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 網データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Trap/NetTrap")]
public class NetData : TrapData
{
    [Header("Effect Settings")]
    public float slowAmount = 0.5f;       // 移動速度減少割合
    public float effectDuration = 3f;     // 効果持続時間
}
