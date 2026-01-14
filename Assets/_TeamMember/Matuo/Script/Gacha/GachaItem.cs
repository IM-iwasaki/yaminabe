using UnityEngine;

/// <summary>
/// ガチャのアイテム情報を保持するクラス
/// </summary>
[System.Serializable]
public class GachaItem {
    [Header("このスキンを使うキャラクター名")]
    public string characterName;

    [Header("アイテム情報")]
    public string itemName;

    [Header("レアリティ")]
    public Rarity rarity;

    [Header("確率設定"), Range(0, 10)]
    public int rate = 1;

    [Header("ガチャ結果用3Dプレハブ")]
    public GameObject resultPrefab;
}