using Mirror;
using UnityEngine;


/// <summary>
/// 武器データ
/// </summary>
public class WeaponData : ScriptableObject, IWeaponInfo {
    public string weaponName;
    public WeaponType type;
    public int damage;
    public float cooldown;

    public int ID;

    [Header("Visual Effects")]
    public EffectType muzzleFlashType = EffectType.Default;
    public EffectType hitEffectType = EffectType.Default;

    public string WeaponName => weaponName;

    [Header("Gun Settings")]
    //  追加：キラ   現在弾薬数
    [System.NonSerialized]public int ammo;
    public int maxAmmo;
    public float reloadTime;

    /// <summary>
    /// 追加：キラ   現在弾薬数を最大弾薬数と同じにする。
    /// </summary>
    public void AmmoReset() {
        ammo = maxAmmo;
    }
}