using UnityEngine;

/// <summary>
/// 近接武器データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/Weapons/MeleeData")]
public class MeleeData : WeaponData
{
    [Header("Melee Settings")]
    [Tooltip("攻撃の範囲")]
    public float range;
    [Tooltip("前方攻撃範囲(半径)")]
    public float meleeAngle;
    [Tooltip("コンボ")]
    public int combo = 1;
    public float comboDelay;

    [Tooltip("SE")]
    public MeleeSEType se;
}
