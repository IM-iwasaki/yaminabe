using System;

/// <summary>
/// ランキング1件分のデータ構造
/// </summary>
[System.Serializable]
public class RankingEntry {
    public string playerName;  // プレイヤー名
    public int score;          // スコア（例：得点）
   

    public RankingEntry(string playerName, int score, float time = 0f, string stageName = "") {
        this.playerName = playerName;
        this.score = score;
      
    }
}
