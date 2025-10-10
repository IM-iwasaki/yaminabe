using UnityEngine;

/// <summary>
/// ガチャのアイテム情報を保持するクラス
/// </summary>
[System.Serializable]
public class GachaItem {
    [Header("アイテム情報")]
    public string itemName;

    [Header("確率設定")]
    [Range(0, 10)]
    public int rate = 1;

    [Header("景品プレハブ(仕様によっては使わないかも)")]
    public GameObject prize;
}