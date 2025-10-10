using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャのアイテムリストをレアリティ別に保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "GachaData", menuName = "Gacha/GachaData")]
public class GachaData : ScriptableObject {
    [Header("レアリティごとのアイテムリスト")]
    public List<GachaItem> commonItems = new List<GachaItem>();
    public List<GachaItem> rareItems = new List<GachaItem>();
    public List<GachaItem> epicItems = new List<GachaItem>();
    public List<GachaItem> legendaryItems = new List<GachaItem>();

    /// <summary>
    /// レアリティに応じたアイテムリストを返す
    /// </summary>
    public List<GachaItem> GetItemsByRarity(Rarity rarity) {
        return rarity switch {
            Rarity.Common => commonItems,
            Rarity.Rare => rareItems,
            Rarity.Epic => epicItems,
            Rarity.Legendary => legendaryItems,
            _ => new List<GachaItem>()
        };
    }
#if UNITY_EDITOR
    // デバッグの時のレアリティ確認用
    public Rarity GetRarityOfItem(GachaItem item) {
        if (commonItems.Contains(item)) return Rarity.Common;
        if (rareItems.Contains(item)) return Rarity.Rare;
        if (epicItems.Contains(item)) return Rarity.Epic;
        if (legendaryItems.Contains(item)) return Rarity.Legendary;
        return Rarity.Common; // デフォルト
    }
#endif
}