using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileCategory {
    Bullet,
    Grenade,
    Trap,
    Magic,
    Other
}

public enum ProjectileType {
    Linear,
    Parabola,
}

public enum EffectType { 
    Default,
    Fire,
    Ice,
    Lightning,
    Explosion,
    Smoke,
}

public enum WeaponType { 
    Melee,
    Gun,
    Magic,
}

public enum SubWeaponType {
    Grenade,
    Trap,
    Item,
    Magic,
}

public enum GrenadeType {
    Flag,       //フラググレネード
    Smoke,      //スモーク
    Stun,       //スタン
    Flash,      //フラッシュ
    Thermite,   //テルミット
}

public enum TrapType {
    Spike,      // ダメージトラップ
    Net,        // 移動制限トラップ
    Landmine,   // 接触で爆発するトラップ
    Fire,       // 炎トラップ
}

public enum ItemType {
    HealthPack,     // HP回復
    Shield,         // 一時的防御バフ
    SpeedBoost,     // 一時的移動速度アップ
    Invisibility,   // 一時的透明化
}

public enum MagicType {
    HealingAura,    // 範囲内の味方を回復
    FireZone,       // 設置型の炎エリアダメージ
    IceZone,        // 設置型の移動速度低下エリア
    LightningField, // 範囲内の敵に連鎖ダメージ
    TeleportPad,    // 設置型のテレポート地点
}