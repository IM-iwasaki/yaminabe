using System;

/// <summary>
/// リザルトデータ変換用
/// </summary>
[Serializable]
public struct ResultScoreData {
    public string PlayerName;  // プレイヤー名
    public int Score;          // スコア値
    public int Kills;　　　　　// キル
    public int Deaths;         // デス
}
