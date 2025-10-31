using UnityEngine;

public abstract class SubWeaponData : ScriptableObject, IWeaponInfo {
    [Header("Basic Info")]
    public string subWeaponName;
    public SubWeaponType type;

    [Header("Stats")]
    public int damage;
    //public float range;
    public float throwForce = 10f;

    [Header("Usage Settings")]
    [Tooltip("最大使用回数（ストック数）")]
    public int maxUses = 3;

    [Tooltip("1回分の使用回数が回復するまでの秒数")]
    public float rechargeTime = 5f;

    [Tooltip("開始時点で満タンかどうか")]
    public bool startFull = true;

    [Header("Projectile Settings")]
    public GameObject ObjectPrefab;
    public float projectileSpeed = 15f;

    [Header("Visual / Audio")]
    public EffectType useEffectType = EffectType.Default;

    public string WeaponName => subWeaponName;

}