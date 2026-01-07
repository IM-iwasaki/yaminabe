using UnityEngine;

/// <summary>
/// 回復アイテムデータ
/// </summary>

[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Item/HealthPack")]
public class HealthPackData : ItemData {
    public int healAmount;
}