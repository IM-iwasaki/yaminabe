using System.Collections.Generic;

[System.Serializable]
public class PlayerData {
    public int currentMoney;                       // プレイヤーの所持金
    public List<PlayerItemStatus> items = new();  // 取得済みアイテムリスト
}