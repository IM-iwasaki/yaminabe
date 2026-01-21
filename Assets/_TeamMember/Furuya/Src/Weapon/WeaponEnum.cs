using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デバッグ確認コード
/// </summary>

#if UNITY_EDITOR
public class ExplosionDebugCircle : MonoBehaviour {
    private float radius;
    private Color color;
    private float duration;
    private float timer;

    public static void Create(Vector3 pos, float radius, Color color, float duration) {
        var obj = new GameObject("ExplosionDebugCircle");
        var circle = obj.AddComponent<ExplosionDebugCircle>();
        circle.radius = radius;
        circle.color = color;
        circle.duration = duration;
        obj.transform.position = pos;
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
    }

    private void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
#endif

/// <summary>
/// Enum宣言用
/// </summary>
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
    DoT,
}

public enum EffectType { 
    Default,
    Fire,
    Ice,
    Lightning,
    Explosion,
    Smoke,
}

public enum GunSEType {
    Gun,
    RocketLauncher,
    Sniper,
}

public enum MagicSEType {
    Fire,
    Ice,
    Water,
}

public enum MeleeSEType {
    Punch,
    Sword,
    Katana,
    Chainsaw,
    Lightsaber,
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
}

public enum MagicType {
    HealingAura,    // 範囲内の味方を回復
    FireZone,       // 設置型の炎エリアダメージ
    IceZone,        // 設置型の移動速度低下エリア
    LightningField, // 範囲内の敵に連鎖ダメージ
    TeleportPad,    // 設置型のテレポート地点
}