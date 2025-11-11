using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// トラップベースデータ
/// </summary>
public class TrapData : SubWeaponData {

    [Header("General Settings")]
    public TrapType trapType;
    public float duration = 0f;           // 効果時間（SpikeやFireなど）
    public float activationDelay = 0f;    // 設置から発動までの遅延
    public bool activationOnce = true;    // 一度だけ発動するか
    public EffectType activationEffect;   // 発動時エフェクト

}
 