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
    // --- KDR をプロパティで計算 ---
    public float KD => (Deaths == 0) ? Kills : (float)Kills / Deaths;
    public int TeamId;// チーム
}
