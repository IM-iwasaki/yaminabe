using System.Collections.Generic;

[System.Serializable]
public class PlayerData {
    public string playerName = "Default";
    public int currentMoney;
    public List<string> items = new();  // アイテム名を直接保持
    /// <summary>
    /// 追加:タハラ　レートをデータとして保存
    /// </summary>
    public int currentRate;
}