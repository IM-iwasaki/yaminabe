using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ガチャのアイテムとレアリティ確率を保持する ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "GachaData", menuName = "ScriptableObject/Gacha/GachaData")]
public class GachaData : ScriptableObject {
    [System.Serializable]
    public class RarityRate {
        public string rarityName;
        public Rarity rarity;
        [Range(0, 100)] public int rate;
    }

    [Header("それぞれのレアリティ出現率(合計100%にして)")]
    public List<RarityRate> rarityRates = new() {
        new RarityRate { rarity = Rarity.Common, rate = 70 },
        new RarityRate { rarity = Rarity.Rare, rate = 20 },
        new RarityRate { rarity = Rarity.Epic, rate = 9 },
        new RarityRate { rarity = Rarity.Legendary, rate = 1 }
    };

    [Header("レアリティごとのアイテムリスト")]
    public List<GachaItem> commonItems = new();
    public List<GachaItem> rareItems = new();
    public List<GachaItem> epicItems = new();
    public List<GachaItem> legendaryItems = new();

    /// <summary>
    /// レアリティに応じたアイテムリストを返す
    /// </summary>
    public List<GachaItem> GetItemsByRarity(Rarity rarity) => rarity switch {
        Rarity.Common => commonItems,
        Rarity.Rare => rareItems,
        Rarity.Epic => epicItems,
        Rarity.Legendary => legendaryItems,
        _ => new List<GachaItem>()
    };

    /// <summary>
    /// GachaData 内のすべてのアイテムをまとめて取得する
    /// </summary>
    /// <returns>Common、Rare、Epic、Legendary の全アイテムを含むリスト</returns>
    public List<GachaItem> GetAllItems() {
        List<GachaItem> allItems = new List<GachaItem>();
        allItems.AddRange(commonItems);
        allItems.AddRange(rareItems);
        allItems.AddRange(epicItems);
        allItems.AddRange(legendaryItems);
        return allItems;
    }

#if UNITY_EDITOR
    private void OnValidate() {
        int total = 0;
        foreach (var r in rarityRates) total += r.rate;
        if (total != 100)
            Debug.LogWarning($"レアリティ出現率の合計が {total}% にねってえるから100%ぴったにして");
    }
#endif
}