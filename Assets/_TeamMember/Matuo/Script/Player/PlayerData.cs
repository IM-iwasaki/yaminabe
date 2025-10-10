using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// セーブするプレイヤーのデータ
/// </summary>
[System.Serializable]
public class PlayerData {
    public int currentMoney;
    public List<string> obtainedItems; // 獲得したアイテム名のリスト
}