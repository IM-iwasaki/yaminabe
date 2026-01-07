using UnityEngine;

/// <summary>
/// シールドアイテムデータ
/// </summary>

[CreateAssetMenu(menuName = "ScriptableObject/SubWeapons/Item/Shield")]
public class ShieldData : ItemData {
    [Header("Shield (Barricade)")]
    public GameObject barricadePrefab;
    public float duration = 5f;
    public float distanceFromPlayer = 1.5f;
}
