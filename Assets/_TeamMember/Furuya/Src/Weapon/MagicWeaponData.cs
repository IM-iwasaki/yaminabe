using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/MagicWeaponData")]
public class MagicWeaponData : WeaponData {
    [Header("Magic Settings")]
    public ParticleSystem chargeEffect;
    public float chargeTime = 1.0f;
    public bool areaEffect; // ”ÍˆÍUŒ‚‚©H
}
