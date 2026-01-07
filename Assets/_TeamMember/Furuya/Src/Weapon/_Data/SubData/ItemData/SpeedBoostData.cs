using UnityEngine;

/// <summary>
/// スピードブーストアイテムデータ
/// </summary>

[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Item/SpeedBoost")]
public class SpeedBoostData : ItemData {
    public float speedMultiplier = 1.5f;
    public float duration;
}
