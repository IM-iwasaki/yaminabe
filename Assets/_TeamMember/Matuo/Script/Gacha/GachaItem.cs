using UnityEngine;

[System.Serializable]
public class GachaItem {
    [Header("武器やスキンの名前とレアリティ")]
    public string itemName;
    public Rarity rarity;
    [Header("そのアイテムの詳しい確率")]
    public int rate;
    [Header("景品")]
    public GameObject prize;
}